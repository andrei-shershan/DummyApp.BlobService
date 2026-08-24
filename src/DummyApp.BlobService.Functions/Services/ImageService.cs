using DummyApp.BlobService.Functions.Models;
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

    public async Task<ImageUploadResult> ProcessAndUploadAsync(string fileName, byte[] imageBytes, string contentType, ImageType imageType, CancellationToken cancellationToken)
    {
        using var image = Image.Load(imageBytes);
        var format = image.Metadata.DecodedImageFormat ?? throw new InvalidOperationException("Unable to determine image format.");

        if (imageType == ImageType.Avatar)
        {
            var avatarImage = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(250, 250),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center
            }));

            await using var avatarStream = new MemoryStream();
            await avatarImage.SaveAsync(avatarStream, format, cancellationToken);
            var avatarBytes = avatarStream.ToArray();
            var avatarUri = await _blobStorageService.UploadAsync(imageType, fileName, avatarBytes, contentType, cancellationToken);

            var thumbnailImage = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(100, 100),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center
            }));

            await using var thumbnailStream = new MemoryStream();
            await thumbnailImage.SaveAsync(thumbnailStream, format, cancellationToken);
            var thumbnailBytes = thumbnailStream.ToArray();

            var thumbnailFileName = GetSmallFileName(fileName);
            var thumbnailUri = await _blobStorageService.UploadAsync(imageType, thumbnailFileName, thumbnailBytes, contentType, cancellationToken);

            return new ImageUploadResult(avatarUri, thumbnailUri);
        }

        var originalUri = await _blobStorageService.UploadAsync(imageType, fileName, imageBytes, contentType, cancellationToken);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(250, 0),
            Mode = ResizeMode.Max
        }));

        await using var smallStream = new MemoryStream();
        await image.SaveAsync(smallStream, format, cancellationToken);
        var smallBytes = smallStream.ToArray();

        var smallFileName = GetSmallFileName(fileName);
        var smallUri = await _blobStorageService.UploadAsync(imageType, smallFileName, smallBytes, contentType, cancellationToken);

        return new ImageUploadResult(originalUri, smallUri);
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
