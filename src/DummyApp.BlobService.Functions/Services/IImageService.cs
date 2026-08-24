using DummyApp.BlobService.Functions.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DummyApp.BlobService.Functions.Services;

public interface IImageService
{
    Task<ImageUploadResult> ProcessAndUploadAsync(string fileName, byte[] imageBytes, string contentType, ImageType imageType, CancellationToken cancellationToken);
}
