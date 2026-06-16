using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.BlobService.Functions.Models;
using DummyApp.BlobService.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DummyApp.BlobService.Functions;

public sealed class UploadImageFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IBlobStorageService _blobStorageService;
    private readonly IUploadImageRequestValidator _validator;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger<UploadImageFunction> _logger;

    public UploadImageFunction(
        IBlobStorageService blobStorageService,
        IUploadImageRequestValidator validator,
        IContentTypeProvider contentTypeProvider,
        ILogger<UploadImageFunction> logger)
    {
        _blobStorageService = blobStorageService;
        _validator = validator;
        _contentTypeProvider = contentTypeProvider;
        _logger = logger;
    }

    [Function("UploadImage")]
    public async Task<HttpResponseData> UploadImage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/upload")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("UploadImage triggered. Method: {Method}, Url: {Url}", req.Method, req.Url);

        UploadImageRequest? uploadRequest;
        try
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);
            uploadRequest = JsonSerializer.Deserialize<UploadImageRequest>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in upload image request.");
            return CreateBadRequest(req, "Invalid JSON in request body.");
        }

        if (!_validator.TryValidate(uploadRequest, out var validationErrorMessage))
        {
            _logger.LogWarning("UploadImage request validation failed: {ValidationError}", validationErrorMessage);
            return CreateBadRequest(req, validationErrorMessage);
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(uploadRequest.Base64Image);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid Base64 image data.");
            return CreateBadRequest(req, "Invalid Base64 image data.");
        }

        var contentType = _contentTypeProvider.GetContentType(uploadRequest.FileName);
        var blobUri = await _blobStorageService.UploadAsync(uploadRequest.FileName, imageBytes, contentType, cancellationToken);

        _logger.LogInformation("File uploaded successfully. FileName: {FileName}, BlobUrl: {BlobUrl}", uploadRequest.FileName, blobUri);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { url = blobUri.AbsoluteUri }, cancellationToken);
        return response;
    }

    private static HttpResponseData CreateBadRequest(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.WriteString(message);
        return response;
    }
}
