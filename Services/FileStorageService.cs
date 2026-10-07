
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
            string fullPath = Path.GetFullPath(Path.Combine())
        }

        public Task<string> SaveAsync(IFormFile file, string subFolder)
        {
            throw new NotImplementedException();
        }
    }
}
