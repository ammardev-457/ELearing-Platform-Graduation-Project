namespace ELProject.ExternalServices
{
    public interface IFileStorageService
    {
        Task<string?> SaveFileInWwwrootAsync(IFormFile file);
    }
}
