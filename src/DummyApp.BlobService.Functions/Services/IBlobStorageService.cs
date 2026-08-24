using DummyApp.BlobService.Functions.Models;

namespace DummyApp.BlobService.Functions.Services;

public interface IBlobStorageService
{
    Task<Uri> UploadAsync(ImageType imageType, string fileName, byte[] content, string contentType, CancellationToken cancellationToken);
}
