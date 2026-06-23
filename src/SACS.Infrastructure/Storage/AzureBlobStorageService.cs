using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string containerName, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(fileName);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(stream, options, cancellationToken);
        return blobClient.Uri.ToString();
    }

    public Task<string> GeneratePresignedUrlAsync(string blobName, string containerName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        // Standard check, requires connection string configuration
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        Uri sasUri;
        if (blobClient.CanGenerateSasUri)
        {
            sasUri = blobClient.GenerateSasUri(sasBuilder);
        }
        else
        {
            // Fallback configuration if CanGenerateSasUri is false during local dev without AccountKey
            // E.g., construct URL with token
            sasUri = new Uri($"{blobClient.Uri}?sas-token-placeholder");
        }
        
        return Task.FromResult(sasUri.ToString());
    }

    public async Task DeleteAsync(string blobName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
