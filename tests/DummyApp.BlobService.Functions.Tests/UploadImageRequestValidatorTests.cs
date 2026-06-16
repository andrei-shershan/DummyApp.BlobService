using DummyApp.BlobService.Functions.Services;
using Xunit;

namespace DummyApp.BlobService.Functions.Tests;

public sealed class UploadImageRequestValidatorTests
{
    [Fact]
    public void TryValidate_ReturnsFalse_WhenRequestIsNull()
    {
        var validator = new UploadImageRequestValidator();

        var result = validator.TryValidate(null, out var errorMessage);

        Assert.False(result);
        Assert.Equal("Request body is required.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenFileNameIsMissing()
    {
        var validator = new UploadImageRequestValidator();
        var request = new Models.UploadImageRequest(Base64Image: "abc", FileName: string.Empty);

        var result = validator.TryValidate(request, out var errorMessage);

        Assert.False(result);
        Assert.Equal("FileName is required.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenBase64ImageIsMissing()
    {
        var validator = new UploadImageRequestValidator();
        var request = new Models.UploadImageRequest(Base64Image: string.Empty, FileName: "image.png");

        var result = validator.TryValidate(request, out var errorMessage);

        Assert.False(result);
        Assert.Equal("Base64Image is required.", errorMessage);
    }

    [Fact]
    public void TryValidate_ReturnsTrue_WhenRequestIsValid()
    {
        var validator = new UploadImageRequestValidator();
        var request = new Models.UploadImageRequest(Base64Image: "YmFzZTY0", FileName: "image.png");

        var result = validator.TryValidate(request, out var errorMessage);

        Assert.True(result);
        Assert.Equal(string.Empty, errorMessage);
    }
}
