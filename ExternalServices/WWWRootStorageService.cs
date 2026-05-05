using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using Microsoft.AspNetCore.Hosting;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Provides file storage functionality for uploaded files.
/// 
/// This service validates the uploaded file (size and extension),
/// generates a unique file name, saves the file to the application's
/// web root directory, and returns the relative file path to be stored
/// in the database.
/// 
/// It abstracts file system operations from controllers to maintain
/// clean architecture and separation of concerns.
/// </summary>
public class WWWRootStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSizeForImage = 2 * 1024 * 1024; // 2MB for images
    private readonly string[] _allowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public WWWRootStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> UploadFileAsync(IFormFile? file, FileType type)
    {
        if (file == null || file.Length == 0)
            return null;

        // Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if(type == FileType.Image)
        {
            // Validate size
            if (file.Length > MaxFileSizeForImage)
                return "File size must be less than 2MB.";

            if (!_allowedExtensions.Contains(extension))
                return "Invalid image format.";

        }

        // Generate unique file name
        var FakeFileName = Path.GetRandomFileName();

        // Generic storage folder
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // بتاعتي file اللي هيكون عليه ال path دا ال
        var filePath = Path.Combine(uploadsFolder, FakeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path (what gets stored in DB)
        return Path.Combine("uploads", FakeFileName).Replace("\\", "/");
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(string fileUrl, FileType type)
    {
        if (string.IsNullOrEmpty(fileUrl) || fileUrl.Contains(".."))
            return null;

        var fullPath = Path.Combine(_environment.WebRootPath, fileUrl);

        if (!File.Exists(fullPath))
            return null;

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();

        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var fileName = Path.GetFileName(fullPath);

        // Still FileStream internally, but returned as Stream
        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true
        );

        return (stream, contentType, fileName);
    }

    public Task<bool> DeleteFileAsync(string fileUrl, FileType type)
    {
        if (string.IsNullOrEmpty(fileUrl) || fileUrl.Contains(".."))
            return Task.FromResult(false);

        var fullPath = Path.Combine(_environment.WebRootPath, fileUrl);

        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);

        return Task.FromResult(true);
    }

}