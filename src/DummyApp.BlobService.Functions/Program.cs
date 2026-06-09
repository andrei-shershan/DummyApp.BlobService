using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config.AddEnvironmentVariables())
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["BlobStorage:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage connection string is not configured. " +
                "Set BlobStorage__ConnectionString.");
        }

        services.AddSingleton(new BlobServiceClient(connectionString));

        var containerName = context.Configuration["BlobStorage:ContainerName"] ?? "artworks";
        var storageUrl = context.Configuration["BlobStorage:StorageUrl"] ?? "default";
        services.AddSingleton(new BlobStorageOptions(storageUrl, containerName));
    })
    .Build();

host.Run();

public sealed record BlobStorageOptions(string StorageUrl, string ContainerName);
