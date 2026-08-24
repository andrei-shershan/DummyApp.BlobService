using DummyApp.BlobService.Functions.Models;

namespace DummyApp.BlobService.Functions.Services;

public interface IImageValidator
{
    bool TryValidate(byte[] imageBytes, string fileName, ImageType imageType, out string errorMessage);
}
