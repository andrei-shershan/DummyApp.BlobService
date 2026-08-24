using DummyApp.BlobService.Functions.Models;

namespace DummyApp.BlobService.Functions.Services;

public sealed class UploadImageRequestValidator : IUploadImageRequestValidator
{
    public bool TryValidate(UploadImageRequest? request, out string errorMessage)
    {
        if (request is null)
        {
            errorMessage = "Request body is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            errorMessage = "FileName is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Base64Image))
        {
            errorMessage = "Base64Image is required.";
            return false;
        }

        if (!Enum.IsDefined(typeof(ImageType), request.ImageType))
        {
            errorMessage = "ImageType is invalid.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
