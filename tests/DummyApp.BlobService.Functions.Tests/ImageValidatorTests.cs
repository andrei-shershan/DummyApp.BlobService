using System.IO;
using System.Linq;
using DummyApp.BlobService.Functions.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace DummyApp.BlobService.Functions.Tests;

public sealed class ImageValidatorTests
{
    [Fact]
    public void TryValidate_ReturnsFalse_WhenImageDataIsMissing()
    {
        var validator = new ImageValidator();

        var result = validator.TryValidate(new byte[0], "image.png", out var errorMessage);

        Assert.False(result);
        Assert.Equal("Image data is required.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenImageSizeExceeds10Mb()
    {
        var validator = new ImageValidator();
        var imageBytes = Enumerable.Repeat((byte)1, 10 * 1024 * 1024 + 1).ToArray();

        var result = validator.TryValidate(imageBytes, "image.png", out var errorMessage);

        Assert.False(result);
        Assert.Equal("Image size must not exceed 10 MB.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenWidthIsTooSmall()
    {
        var validator = new ImageValidator();
        using var image = new Image<Rgba32>(1000, 1414);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        var imageBytes = stream.ToArray();

        var result = validator.TryValidate(imageBytes, "image.png", out var errorMessage);

        Assert.False(result);
        Assert.Equal("Image width must be at least 1024 pixels.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenImageIsNotA4Portrait()
    {
        var validator = new ImageValidator();
        using var image = new Image<Rgba32>(1200, 1500);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        var imageBytes = stream.ToArray();

        var result = validator.TryValidate(imageBytes, "image.png", out var errorMessage);

        Assert.False(result);
        Assert.Equal("Image must have A4 portrait proportions.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsTrue_ForValidA4PortraitImage()
    {
        var validator = new ImageValidator();
        using var image = new Image<Rgba32>(1200, 1697);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        var imageBytes = stream.ToArray();

        var result = validator.TryValidate(imageBytes, "image.png", out var errorMessage);

        Assert.True(result);
        Assert.Equal(string.Empty, errorMessage);
    }
}
