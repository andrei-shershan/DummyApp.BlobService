using DummyApp.BlobService.Functions.Models;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace DummyApp.BlobService.Functions.Services;

public sealed class ImageValidator : IImageValidator
{
    private const int MinWidth = 1024;
    private const long MaxImageSizeBytes = 10L * 1024 * 1024;
    private static readonly double A4PortraitAspectRatio = 1.0 / Math.Sqrt(2);
    private const double AspectRatioTolerance = 0.05;

    public bool TryValidate(byte[] imageBytes, string fileName, ImageType imageType, out string errorMessage)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            errorMessage = "Image data is required.";
            return false;
        }

        if (imageBytes.Length > MaxImageSizeBytes)
        {
            errorMessage = "Image size must not exceed 10 MB.";
            return false;
        }

        try
        {
            using var image = Image.Load(imageBytes);

            if (imageType == ImageType.Artwork)
            {
                if (image.Width < MinWidth)
                {
                    errorMessage = $"Image width must be at least {MinWidth} pixels.";
                    return false;
                }

                if (image.Height <= image.Width)
                {
                    errorMessage = "Image must be in portrait orientation.";
                    return false;
                }

                var aspectRatio = image.Width / (double)image.Height;
                if (Math.Abs(aspectRatio - A4PortraitAspectRatio) > AspectRatioTolerance)
                {
                    errorMessage = "Image must have A4 portrait proportions.";
                    return false;
                }
            }
            else if (imageType == ImageType.Avatar)
            {
                // Avatar validation currently only guarantees a valid image and size; additional rules can be added later.
                if (image.Width < 64 || image.Height < 64)
                {
                    errorMessage = "Avatar image must be at least 64x64 pixels.";
                    return false;
                }
            }
            else
            {
                errorMessage = "Unsupported image type.";
                return false;
            }
        }
        catch (UnknownImageFormatException)
        {
            errorMessage = "Invalid image format or corrupted image.";
            return false;
        }
        catch (Exception)
        {
            errorMessage = "Invalid image format or corrupted image.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
