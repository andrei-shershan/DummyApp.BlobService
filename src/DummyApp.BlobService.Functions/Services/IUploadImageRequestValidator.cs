using DummyApp.BlobService.Functions.Models;

namespace DummyApp.BlobService.Functions.Services;

public interface IUploadImageRequestValidator
{
    bool TryValidate(UploadImageRequest? request, out string errorMessage);
}
