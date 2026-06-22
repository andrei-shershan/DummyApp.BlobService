using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.BlobService.Functions.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace DummyApp.BlobService.Functions.Tests;

public sealed class ImageServiceTests
{
    [Fact]
    public async Task ProcessAndUploadAsync_UploadsOriginalAndThumbnailWithSmallSuffix()
    {
        var uploadCalls = new List<UploadCall>();
        var blobStorageService = new FakeBlobStorageService(uploadCalls);
        var imageService = new ImageService(blobStorageService);

        var fileName = "picture.png";
        var contentType = "image/png";
        var imageBytes = await CreateTestPngImageAsync();

        var result = await imageService.ProcessAndUploadAsync(fileName, imageBytes, contentType, CancellationToken.None);

        Assert.Equal(new Uri("https://example.com/picture.png"), result.OriginalUri);
        Assert.Equal(new Uri("https://example.com/picture-small.png"), result.ThumbnailUri);
        Assert.Equal(2, uploadCalls.Count);

        Assert.Equal(fileName, uploadCalls[0].FileName);
        Assert.Equal(contentType, uploadCalls[0].ContentType);
        Assert.Equal(imageBytes, uploadCalls[0].Content);

        Assert.Equal("picture-small.png", uploadCalls[1].FileName);
        Assert.Equal(contentType, uploadCalls[1].ContentType);
        Assert.NotEqual(imageBytes, uploadCalls[1].Content);
        Assert.True(uploadCalls[1].Content.Length > 0);
    }

    private static async Task<byte[]> CreateTestPngImageAsync()
    {
        await using var memoryStream = new MemoryStream();
        using var image = new Image<Rgba32>(10, 10);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image[x, y] = new Rgba32(255, 0, 0, 255);
            }
        }

        await image.SaveAsPngAsync(memoryStream);

        return memoryStream.ToArray();
    }

    private sealed class FakeBlobStorageService : IBlobStorageService
    {
        private readonly List<UploadCall> _uploadCalls;

        public FakeBlobStorageService(List<UploadCall> uploadCalls)
        {
            _uploadCalls = uploadCalls;
        }

        public Task<Uri> UploadAsync(string fileName, byte[] content, string contentType, CancellationToken cancellationToken)
        {
            _uploadCalls.Add(new UploadCall(fileName, content, contentType));
            return Task.FromResult(new Uri($"https://example.com/{fileName}"));
        }
    }

    private sealed record UploadCall(string FileName, byte[] Content, string ContentType);
}
