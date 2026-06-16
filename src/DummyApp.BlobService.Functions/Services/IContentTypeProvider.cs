namespace DummyApp.BlobService.Functions.Services;

public interface IContentTypeProvider
{
    string GetContentType(string fileName);
}
