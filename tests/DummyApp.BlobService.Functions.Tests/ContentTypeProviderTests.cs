using DummyApp.BlobService.Functions.Services;
using Xunit;

namespace DummyApp.BlobService.Functions.Tests;

public sealed class ContentTypeProviderTests
{
    [Theory]
    [InlineData("picture.jpg", "image/jpeg")]
    [InlineData("picture.jpeg", "image/jpeg")]
    [InlineData("picture.png", "image/png")]
    [InlineData("picture.gif", "image/gif")]
    [InlineData("picture.bmp", "image/bmp")]
    [InlineData("picture.webp", "image/webp")]
    [InlineData("document.txt", "application/octet-stream")]
    public void GetContentType_ReturnsExpectedMimeType(string fileName, string expected)
    {
        var provider = new ContentTypeProvider();

        var contentType = provider.GetContentType(fileName);

        Assert.Equal(expected, contentType);
    }
}
