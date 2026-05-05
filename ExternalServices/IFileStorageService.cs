using ELProject.Domain.Enums;

namespace ELProject.ExternalServices
{
    public interface IFileStorageService
    {
        Task<string?> UploadFileAsync(IFormFile file, FileType type);
        Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(string fileUrl, FileType type);
        Task<bool> DeleteFileAsync(string fileUrl, FileType type);

    }
}
