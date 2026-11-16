namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class FileStorageService : IFileStorageService
    {
        private const long MaxThumbnailSizeBytes = 5 * 1024 * 1024; // 5 MB
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<FileStorageService> logger;
        public FileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<FileStorageService> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        public async Task<bool> IsValidThumbnailAsync(ThumbnailUpload? file)
        {
            if (file == null ||
                file.Content == null ||
               !file.Content.CanRead ||
               !file.Content.CanSeek ||
                file.Length <= 0 ||
                file.Length > MaxThumbnailSizeBytes)
            {
                return false;
            }

            if (!string.Equals(Path.GetExtension(file.FileName), ".png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            byte[] pngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            byte[] fileSignature = new byte[pngSignature.Length];

            long originalPosition = file.Content.Position;

            try
            {
                int bytesRead = await file.Content.ReadAsync(fileSignature, 0, fileSignature.Length);

                return bytesRead == pngSignature.Length &&
                       fileSignature.SequenceEqual(pngSignature);
            }
            finally
            {
                file.Content.Position = originalPosition;
            }
        }

        public bool DeleteFile(string relativePath)
        {
            string? physicalPath = GetPhysicalPath(relativePath);

            if (physicalPath == null)
            {
                logger.LogWarning($"Skipped deleting file, invalid path: {relativePath}");
                return false;
            }

            if (!File.Exists(physicalPath))
            {
                return true;
            }

            try
            {
                File.Delete(physicalPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Could not delete file {physicalPath}");
                return false;
            }

            return true;
        }

        public string? GetPhysicalPath(string relativePath)
        {

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            string root = Path.GetFullPath(webHostEnvironment.WebRootPath);
            string trimmedPath = relativePath.TrimStart('/', '\\');
            string fullpath = Path.GetFullPath(Path.Combine(root, trimmedPath));

            if (!fullpath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return fullpath;
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

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string subFolder)
        {
            if (fileStream == null || !fileStream.CanRead)
            {
                throw new ArgumentException("Invalid file stream.", nameof(fileStream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(subFolder))
            {
                throw new ArgumentException("Subfolder is required.", nameof(subFolder));
            }

            // Keep only the file name, not any supplied directory path.
            fileName = Path.GetFileName(fileName);

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("File extension is required.", nameof(fileName));
            }

            // Generate a unique stored filename.
            string storedFileName = Guid.NewGuid() + extension;

            // Build the relative storage path.
            string relativePath = Path.Combine("uploads", subFolder, storedFileName).Replace('\\', '/');

            // Resolve and validate the physical path.
            string? fullPath = GetPhysicalPath(relativePath);

            if (fullPath == null)
            {
                throw new InvalidOperationException("Invalid file storage path.");
            }

            // Create the destination directory.
            string? directory = Path.GetDirectoryName(fullPath);

            if (directory == null)
            {
                throw new InvalidOperationException("Invalid destination directory.");
            }

            Directory.CreateDirectory(directory);

            // Save the stream.
            using (var outputStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            return "/" + relativePath;
        }
    }
}
