using ELProject.ExternalServices;
using Microsoft.AspNetCore.Hosting;

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

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveFileAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        // ✅ Validate size
        if (file.Length > MaxFileSize)
            return "File size must be less than 5MB.";

        // ✅ Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
            return "Invalid image format.";

        // ✅ Generate unique file name
        var fileName = $"{Guid.NewGuid()}{extension}";

        // ✅ Generic storage folder
        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ✅ Return relative path (what gets stored in DB)
        return Path.Combine("uploads", fileName)
                   .Replace("\\", "/");
    }
}