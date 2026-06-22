using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace DummyApp.BlobService.Functions.Services;

public sealed class ImageService : IImageService
{
    private readonly IBlobStorageService _blobStorageService;

    public ImageService(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public async Task<ImageUploadResult> ProcessAndUploadAsync(string fileName, byte[] imageBytes, string contentType, CancellationToken cancellationToken)
    {
        var originalUri = await _blobStorageService.UploadAsync(fileName, imageBytes, contentType, cancellationToken);

        using var image = Image.Load(imageBytes);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 0),
            Mode = ResizeMode.Max
        }));

        await using var thumbnailStream = new MemoryStream();
        var format = image.Metadata.DecodedImageFormat ?? throw new InvalidOperationException("Unable to determine image format.");
        await image.SaveAsync(thumbnailStream, format, cancellationToken);
        var thumbnailBytes = thumbnailStream.ToArray();

        var thumbnailFileName = GetSmallFileName(fileName);
        var thumbnailUri = await _blobStorageService.UploadAsync(thumbnailFileName, thumbnailBytes, contentType, cancellationToken);

        return new ImageUploadResult(originalUri, thumbnailUri);
    }

    private static string GetSmallFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            return fileName + "-small";
        }

        var nameWithoutExtension = fileName[..^extension.Length];
        return $"{nameWithoutExtension}-small{extension}";
    }
}
