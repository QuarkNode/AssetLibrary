namespace PartManagementSystem.Services.Contracts
{
    public interface IFileStorageService
    {
        /* Description: Saves a file to disk, deletes it, gives back its path; Never touches the database. */

        Task<string> SaveAsync(IFormFile file, string subFolder);
        string? GetPhysicalPath(string relativePath);
        void DeleteFile(string relativePath);
    }
}
