
using Microsoft.AspNetCore.Http;

namespace AssetLibrary.Services.Contracts
{
    public interface IFileStorageService
    {
        public bool DeleteFile(string relativePath);
        public Task<string> SaveAsync(IFormFile file, string subFolder);
        public string? GetPhysicalPath(string relativePath);
    }
}
