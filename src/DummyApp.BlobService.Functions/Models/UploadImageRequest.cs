namespace DummyApp.BlobService.Functions.Models;

public sealed record UploadImageRequest(string Base64Image, string FileName);
