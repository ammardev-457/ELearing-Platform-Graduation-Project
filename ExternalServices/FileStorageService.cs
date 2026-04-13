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
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private readonly string[] _allowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            return null;

        // Validate size
        if (image.Length > MaxFileSize)
            return "File size must be less than 2MB.";

        // Validate extension
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
            return "Invalid image format.";

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
            await image.CopyToAsync(stream);
        }

        // Return relative path (what gets stored in DB)
        return Path.Combine("uploads", FakeFileName).Replace("\\", "/");
    }

    
}