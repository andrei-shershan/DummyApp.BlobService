namespace DummyApp.BlobService.Functions.Services;

public interface IImageValidator
{
    bool TryValidate(byte[] imageBytes, string fileName, out string errorMessage);
}
