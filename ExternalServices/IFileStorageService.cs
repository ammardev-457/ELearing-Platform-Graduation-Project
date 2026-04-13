namespace ELProject.ExternalServices
{
    public interface IFileStorageService
    {
        Task<string?> SaveImageAsync(IFormFile file);
    }
}
