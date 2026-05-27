using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var blobStorageUri = configuration["BlobStorageUri"];

            if (!string.IsNullOrEmpty(blobStorageUri))
            {
                // Deployed: use managed identity
                return new BlobServiceClient(new Uri(blobStorageUri), new DefaultAzureCredential());
            }

            // Local: fall back to connection string (UseDevelopmentStorage=true)
            var connectionString = configuration["AzureWebJobsStorage"] ?? string.Empty;
            return new BlobServiceClient(connectionString);
        });
    })
    .Build();

host.Run();
