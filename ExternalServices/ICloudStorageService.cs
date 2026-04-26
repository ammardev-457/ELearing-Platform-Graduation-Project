using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ELProject.Domain.Enums;

namespace ELProject.ExternalServices
{
    public interface ICloudStorageService
    {
        // For SAS uploading from client to cloud
        string GenerateUploadSas(string fileName, FileType type);
        
        // For Proxy uploading from client to server to cloud
        Task<string?> UploadFileAsync(IFormFile file, FileType type);
        Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(string fileUrl, FileType type);
    }
}
