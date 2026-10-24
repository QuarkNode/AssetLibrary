namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Services.Contracts;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<FileStorageService> logger;
        public FileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<FileStorageService> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }
        public async Task<bool> DeleteFile(string relativePath)
        {
            string? physicalPath = GetPhysicalPath(relativePath);

            if (physicalPath == null)
            {
                logger.LogWarning($"Skipped deleting file, invalid path: {relativePath}");
                return await Task.FromResult(false);
            }

            if (!File.Exists(physicalPath))
            {
                return await Task.FromResult(false);
            }

            try
            {
                File.Delete(physicalPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Could not delete file {physicalPath}");
                return await Task.FromResult(false);
            }

            return true;

            //if (physicalPath != null && File.Exists(physicalPath))
            //{
            //    File.Delete(physicalPath);
            //}
        }

        public string? GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            string trimmedPath = relativePath.TrimStart('/', '\\');

            return Path.Combine(webHostEnvironment.WebRootPath, "uploads", "assets", trimmedPath);
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder)
        {
            // Path: wwwroot/uploads/{subFolder="assets/"}

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
