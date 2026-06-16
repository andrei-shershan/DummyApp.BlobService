using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using DummyApp.BlobService.Functions.Options;

namespace DummyApp.BlobService.Functions.Services;

public sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _options;

    public BlobStorageService(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public async Task<Uri> UploadAsync(string fileName, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(fileName);

        await using var stream = new MemoryStream(content);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blobClient.Uri;
    }
}
