namespace DummyApp.BlobService.Functions.Services;

public interface IBlobStorageService
{
    Task<Uri> UploadAsync(string fileName, byte[] content, string contentType, CancellationToken cancellationToken);
}
