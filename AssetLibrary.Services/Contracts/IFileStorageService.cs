
using Microsoft.AspNetCore.Http;

namespace AssetLibrary.Services.Contracts
{
    public interface IFileStorageService
    {
        /* Description: Saves a file to disk, deletes it, gives back its path; Never touches the database. */

        public bool DeleteFile(string relativePath);
        public Task<string> SaveAsync(IFormFile file, string subFolder);
        public string? GetPhysicalPath(string relativePath);
    }
}
