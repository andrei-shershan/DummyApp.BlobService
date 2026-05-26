using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DummyApp.BlobService.Functions;

public class BlobServiceFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobServiceFunction> _logger;

    public BlobServiceFunction(BlobServiceClient blobServiceClient, ILogger<BlobServiceFunction> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    [Function("BlobService")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blobservice")] HttpRequestData req)
    {
        _logger.LogInformation("BlobService function triggered.");

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync("hello world from BlobService Functions");

        return response;
    }
}
