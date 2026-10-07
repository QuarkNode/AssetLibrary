
namespace PartManagementSystem.Services
{
    using PartManagementSystem.Services.Contracts;
    public class FileStorageService : IFileStorageService
    {
        public void DeleteFile(string relativePath)
        {
            throw new NotImplementedException();
        }

        public string? GetPhysicalPath(string relativePath)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveAsync(IFormFile file, string subFolder)
        {
            throw new NotImplementedException();
        }
    }
}
