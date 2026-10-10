
namespace PartManagementSystem.Services
{
    using PartManagementSystem.Data;
    using PartManagementSystem.Services.Contracts;
    public class FileStorageService : IFileStorageService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        public FileStorageService(AssetLibraryDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public void DeleteFile(string relativePath)
        {
            string? physicalPath = GetPhysicalPath(relativePath);

            if (physicalPath != null && File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        public string? GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            string trimmedPath = relativePath.TrimStart('/', '\\');

            return Path.Combine(webHostEnvironment.WebRootPath, trimmedPath);
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder)
        {
            // Path: wwwroot/uploads/{subFolder}

            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", subFolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string storedFileName = Guid.NewGuid() + extension;
            string fullPath = Path.Combine(uploadsFolder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = Path.Combine("/uploads", subFolder, storedFileName).Replace('\\', '/');
            return relativePath;
        }
    }
}
