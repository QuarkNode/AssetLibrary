namespace AssetLibrary.Services.Contracts
{
    using AssetLibrary.Services.Models;
    using Microsoft.AspNetCore.Http;
    public interface IFileStorageService
    {
        public Task<bool> IsValidThumbnailAsync(ThumbnailUpload? file);
        public bool DeleteFile(string relativePath);
        public Task<string> SaveAsync(IFormFile file, string subFolder);
        public Task<string> SaveAsync(Stream fileStream, string fileName, string subFolder);
        public string? GetPhysicalPath(string relativePath);
    }
}
