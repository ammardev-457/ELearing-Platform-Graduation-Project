using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ELProject.Domain.Enums;
using ELProject.ExternalServices;
using System.Data.Common;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _serviceClient;

    public AzureBlobStorageService(IConfiguration config)
    {
        _serviceClient = new BlobServiceClient(
            config.GetSection("AzureBlobStorage")["ConnectionString"]);
    }

    public async Task<string?> UploadFileAsync(IFormFile file, FileType type)
    {
        if (file == null || file.Length == 0)
            return null;

        var containerName = type switch
        {
            FileType.Image => "images",
            FileType.Pdf => "pdfs",
            FileType.Video => "videos",
            _ => "misc"
        };

        BlobContainerClient containerClient = _serviceClient.GetBlobContainerClient(containerName);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            }
        };

        await blobClient.UploadAsync(stream, options);

        return blobClient.Uri.ToString();
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(string fileUrl, FileType type)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return null;

        var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

        var containerName = type switch
        {
            FileType.Image => "images",
            FileType.Pdf => "pdfs",
            FileType.Video => "videos",
            _ => "misc"
        };

        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
            return null;

        var response = await blobClient.DownloadStreamingAsync();

        return (
            response.Value.Content,
            response.Value.Details.ContentType,
            fileName
        );
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, FileType type)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return false;

        var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

        var containerName = type switch
        {
            FileType.Image => "images",
            FileType.Pdf => "pdfs",
            FileType.Video => "videos",
            _ => "misc"
        };

        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.DeleteIfExistsAsync();

        return response.Value; // true if deleted, false if not found
    }

    public async Task<string?> GenerateSasUrlAsync(string fileUrl, FileType type, int expiresInMinutes = 60)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return null;

        var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
        var containerName = type switch
        {
            FileType.Image => "images",
            FileType.Pdf => "pdfs",
            FileType.Video => "videos",
            _ => "misc"
        };

        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
            return null;

        // Build the SAS token — read-only, expires in N minutes
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fileName,
            Resource = "b",                                         // "b" = blob
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Generate the full signed URI
        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

}