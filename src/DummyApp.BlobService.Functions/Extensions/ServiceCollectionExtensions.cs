using Azure.Storage.Blobs;
using DummyApp.BlobService.Functions.Options;
using DummyApp.BlobService.Functions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DummyApp.BlobService.Functions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .Bind(configuration.GetSection(BlobStorageOptions.SectionName))
            .ValidateDataAnnotations();

        services
            .AddOptions<KeyVaultOptions>()
            .Bind(configuration.GetSection(KeyVaultOptions.SectionName));

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException(
                    $"Azure Blob Storage connection string is not configured. " +
                    $"Set {BlobStorageOptions.SectionName}:{nameof(BlobStorageOptions.ConnectionString)} or store it in Key Vault.");
            }

            if (options.Use2024Version)
            {
                var blobClientOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_02_04);
                return new BlobServiceClient(options.ConnectionString, blobClientOptions);
            }

            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IContentTypeProvider, ContentTypeProvider>();
        services.AddSingleton<IUploadImageRequestValidator, UploadImageRequestValidator>();

        return services;
    }
}
