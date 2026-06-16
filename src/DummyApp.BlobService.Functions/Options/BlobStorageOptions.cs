using System.ComponentModel.DataAnnotations;

namespace DummyApp.BlobService.Functions.Options;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    [Required]
    public string ContainerName { get; init; } = "artworks";

    [Required]
    public string StorageUrl { get; init; } = "default";

    public bool Use2024Version { get; init; }
}
