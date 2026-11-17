
namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models;
    using Microsoft.EntityFrameworkCore;

    public class ThumbnailService : IThumbnailService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IFileStorageService fileStorageService;
        private readonly IThumbnailRendererService thumbnailRenderer;

        public ThumbnailService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService, IThumbnailRendererService thumbnailRenderer)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
            this.thumbnailRenderer = thumbnailRenderer;
        }


        public Task<byte[]> GenerateAssetThumbnailAsync(string rendererUrl, string modelUrl)
        {
            return this.thumbnailRenderer.RenderAssetAsync(rendererUrl, modelUrl);
        }

        public Task<byte[]> GenerateProjectThumbnailAsync(string rendererUrl, IReadOnlyCollection<string> modelUrls)
        {
            return this.thumbnailRenderer.RenderProjectAsync(rendererUrl, modelUrls);
        }

        public async Task<string> SaveAssetThumbnailAsync(int revisionId, ThumbnailUpload thumbnail)
        {
            var revision = await this.dbContext.Revisions
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.RevisionId == revisionId);

            if (revision == null)
            {
                throw new InvalidOperationException($"Revision {revisionId} was not found.");
            }

            string? oldPath = revision.ThumbnailPath;
            string newPath = await SaveThumbnailFileAsync(thumbnail, $"assets/{revision.AssetId}/thumbnails");

            try
            {
                revision.ThumbnailPath = newPath;
                revision.ThumbnailGeneratedOn = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();
            }
            catch
            {
                fileStorageService.DeleteFile(newPath);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldPath) && oldPath != newPath)
            {
                fileStorageService.DeleteFile(oldPath);
            }

            return newPath;
        }

        public async Task<string> SaveProjectThumbnailAsync(int projectId, ThumbnailUpload thumbnail)
        {
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                throw new InvalidOperationException($"Project {projectId} was not found.");
            }

            string? oldPath = project.ThumbnailPath;
            string newPath = await SaveThumbnailFileAsync(thumbnail, $"projects/{projectId}/thumbnails");

            try
            {
                project.ThumbnailPath = newPath;

                await dbContext.SaveChangesAsync();
            }
            catch
            {
                fileStorageService.DeleteFile(newPath);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldPath) && oldPath != newPath)
            {
                fileStorageService.DeleteFile(oldPath);
            }

            return newPath;
        }

        public async Task ClearProjectThumbnailAsync(int projectId)
        {
            var project = await dbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                throw new InvalidOperationException($"Project {projectId} was not found.");
            }

            string? oldPath = project.ThumbnailPath;

            project.ThumbnailPath = null;
            project.ThumbnailGeneratedOn = null;

            await dbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldPath))
            {
                fileStorageService.DeleteFile(oldPath);
            }
        }

        private async Task<string> SaveThumbnailFileAsync(ThumbnailUpload thumbnail, string subFolder)
        {
            bool isValid = await fileStorageService.IsValidThumbnailAsync(thumbnail);

            if (!isValid)
            {
                throw new InvalidDataException("The supplied thumbnail is not a valid PNG file.");
            }

            thumbnail.Content.Position = 0;

            return await fileStorageService.SaveAsync(thumbnail.Content, thumbnail.FileName, subFolder);
        }
        public void DeleteThumbnail(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            fileStorageService.DeleteFile(relativePath);
        }
    }
}
