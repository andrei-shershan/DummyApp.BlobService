using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();

        if (!context.HostingEnvironment.IsDevelopment())
        {
            var builtConfig = config.Build();
            var keyVaultUrl = builtConfig["KeyVault:Url"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                var credential = string.IsNullOrEmpty(clientId)
                    ? new ManagedIdentityCredential()
                    : new ManagedIdentityCredential(clientId);

                config.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
            }
        }
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["BlobStorage:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage connection string is not configured. " +
                "Set BlobStorage__ConnectionString or store BlobStorage--ConnectionString in Key Vault.");
        }

        var useB2024Version = context.Configuration.GetValue<bool>("BlobStorage:Use2024Version");
        if (useB2024Version)
        {
            var blobClientOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_02_04);
            services.AddSingleton(new BlobServiceClient(connectionString, blobClientOptions));
        }
        else
        {
            services.AddSingleton(new BlobServiceClient(connectionString));
        }

        var containerName = context.Configuration["BlobStorage:ContainerName"] ?? "artworks";
        var storageUrl = context.Configuration["BlobStorage:StorageUrl"] ?? "default";
        services.AddSingleton(new BlobStorageOptions(storageUrl, containerName));
    })
    .Build();

host.Run();

public sealed record BlobStorageOptions(string StorageUrl, string ContainerName);
