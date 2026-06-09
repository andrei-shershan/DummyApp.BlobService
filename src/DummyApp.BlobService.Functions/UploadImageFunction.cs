using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DummyApp.BlobService.Functions;

public class UploadImageFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _storageOptions;
    private readonly ILogger<UploadImageFunction> _logger;

    public UploadImageFunction(
        BlobServiceClient blobServiceClient,
        BlobStorageOptions storageOptions,
        ILogger<UploadImageFunction> logger)
    {
        _blobServiceClient = blobServiceClient;
        _storageOptions = storageOptions;
        _logger = logger;
    }

    [Function("UploadImage")]
    public async Task<HttpResponseData> UploadImage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/upload")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("UploadImage triggered. Method: {Method}, URL: {Url}", req.Method, req.Url);
        _logger.LogInformation("Request headers: {Headers}", string.Join("; ", req.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

        UploadImageRequest? uploadRequest;
        string body;
        try
        {
            using var reader = new StreamReader(req.Body);
            body = await reader.ReadToEndAsync(cancellationToken);
            _logger.LogInformation("Request body length: {Length} chars. Body: {Body}", body.Length, body);
            uploadRequest = JsonSerializer.Deserialize<UploadImageRequest>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in upload image request.");
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid request body.", cancellationToken);
            return badResponse;
        }

        _logger.LogInformation("Deserialized request: FileName={FileName}, Base64Image length={Base64Length}",
            uploadRequest?.FileName,
            uploadRequest?.Base64Image?.Length ?? 0);

        if (uploadRequest is null
            || string.IsNullOrEmpty(uploadRequest.Base64Image)
            || string.IsNullOrEmpty(uploadRequest.FileName))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Base64Image and FileName are required.", cancellationToken);
            return badResponse;
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(uploadRequest.Base64Image);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid Base64 image data.");
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid Base64 image data.", cancellationToken);
            return badResponse;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(uploadRequest.FileName);
        var contentType = GetContentType(uploadRequest.FileName);

        _logger.LogInformation("Uploading file to Blob Storage. FileName: {FileName}, ContainerName: {ContainerName}, ContentType: {ContentType}",
            uploadRequest.FileName, _storageOptions.ContainerName, contentType);

        using var stream = new MemoryStream(imageBytes);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        _logger.LogInformation("File uploaded successfully. FileName: {FileName}, ContainerName: {ContainerName}", uploadRequest.FileName, _storageOptions.ContainerName);
        _logger.LogInformation("File URI: {FileUri}", blobClient.Uri.AbsolutePath);

        var ifExist = await blobClient.ExistsAsync(cancellationToken);
        _logger.LogInformation("File exists: {IfExist}", ifExist.Value);
        _logger.LogInformation("Image uploaded to blob storage: {BlobUrl}", blobClient.Uri);

        var relativeUrl = "/" + uploadRequest.FileName.TrimStart('/');
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { url = relativeUrl }, cancellationToken);
        return response;
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private sealed record UploadImageRequest(string Base64Image, string FileName);
}
