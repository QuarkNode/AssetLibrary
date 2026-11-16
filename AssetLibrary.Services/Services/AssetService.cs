namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Data.Models;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Revision;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AssetService : IAssetService
    {
        private readonly AssetLibraryDbContext dbContext;

        private readonly ILogger<AssetService> logger;
        private readonly IFileStorageService fileStorageService;
        public AssetService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService, ILogger<AssetService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.fileStorageService = fileStorageService;

        }
        public async Task<Asset?> GetAssetDetailsAsync(int assetId)
        {
            var asset = await dbContext.Assets
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.Material)
                .Include(a => a.Owner)
                .Include(a => a.CurrentRevision)
                .Include(a => a.Revisions)
                    .ThenInclude(r => r.UploadedByUser)
                .FirstOrDefaultAsync(a => a.AssetId == assetId && !a.IsDeleted);

            return asset;
        }

        public async Task<Asset?> GetAssetDetailsAsync(int assetId, int userId)
        {
            var asset = await dbContext.Assets
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.Material)
                .Include(a => a.Owner)
                .Include(a => a.CurrentRevision)
                .Include(a => a.Revisions)
                    .ThenInclude(r => r.UploadedByUser)
                .FirstOrDefaultAsync(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted);

            return asset;
        }

        public async Task<DeleteAssetViewModel?> GetDeleteAssetViewModelAsync(int assetId, int userId)
        {
            var model = await dbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted)
                .Select(a => new DeleteAssetViewModel
                {
                    AssetId = a.AssetId,
                    AssetName = a.AssetName,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.ProjectName,
                    ThumbnailPath = a.CurrentRevision != null ? a.CurrentRevision.ThumbnailPath : null
                })
                .FirstOrDefaultAsync();

            return model;
        }
        public async Task<RevisionFileViewModel?> GetRevisionFileAsync(int revisionId, int userId)
        {
            var revision = await dbContext.Revisions
                .AsNoTracking()
                .Where(r => r.RevisionId == revisionId
                            && r.Asset.OwnerId == userId
                            && !r.Asset.IsDeleted)
                .Select(r => new RevisionFileViewModel
                {
                    RevisionId = r.RevisionId,
                    AssetId = r.Asset.AssetId,
                    FilePath = r.FilePath,
                    ThumbnailPath = r.ThumbnailPath ?? string.Empty,
                    OriginalFIleName = r.OriginalFileName,
                    FileExtension = r.FileExtension
                })
                .FirstOrDefaultAsync();

            return revision;
        }

        public async Task<RevisionFileViewModel?> GetCurrentRevisionFileAsync(int assetId, int userId)
        {
            var revision = await dbContext.Revisions
                .AsNoTracking()
                .Where(r => r.AssetId == assetId
                            && r.Asset.OwnerId == userId
                            && !r.Asset.IsDeleted
                            && r.RevisionId == r.Asset.CurrentRevisionId)
                .Select(r => new RevisionFileViewModel
                {
                    RevisionId = r.RevisionId,
                    AssetId = r.AssetId,
                    FilePath = r.FilePath,
                    ThumbnailPath = r.ThumbnailPath ?? string.Empty,
                    OriginalFIleName = r.OriginalFileName,
                    FileExtension = r.FileExtension
                })
                .FirstOrDefaultAsync();

            return revision;
        }

        public async Task<string?> DeleteRevisionAsync(int revisionId, int userId)
        {
            var revision = await dbContext.Revisions
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.RevisionId == revisionId && r.Asset.OwnerId == userId && !r.Asset.IsDeleted);

            if (revision == null)
            {
                return null;
            }

            string filePath = revision.FilePath;
            var asset = revision.Asset;

            if (asset.CurrentRevisionId == revision.RevisionId)
            {
                var previousRevision = await dbContext.Revisions
                    .Where(r =>
                        r.AssetId == asset.AssetId &&
                        r.RevisionId != revisionId)
                    .OrderByDescending(r => r.RevisionNumber)
                    .FirstOrDefaultAsync();

                asset.CurrentRevision = previousRevision;
                asset.CurrentRevisionId = previousRevision?.RevisionId;
            }

            dbContext.Revisions.Remove(revision);
            await dbContext.SaveChangesAsync();

            return filePath;
        }

        public async Task<AddRevisionViewModel?> GetAddRevisionViewModelAsync(int assetId, int userId)
        {

            return await dbContext.Assets
                 .AsNoTracking()
                 .Where(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted)
                 .Select(a => new AddRevisionViewModel
                 {
                     AssetId = a.AssetId,
                     ProjectId = a.ProjectId,
                     AssetName = a.AssetName,
                     NextRevisionNumber = (a.Revisions.Max(r => (int?)r.RevisionNumber) ?? 0) + 1
                 })
                 .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateRevisionAsync(AddRevisionViewModel model, ThumbnailUpload? thumbnail, int userId)
        {
            var asset = await dbContext.Assets
                .FirstOrDefaultAsync(a =>
                    a.AssetId == model.AssetId &&
                    a.OwnerId == userId &&
                    !a.IsDeleted);

            if (asset == null)
            {
                return false;
            }

            int nextNumber = await GetNextRevisionNumberAsync(asset.AssetId);

            string relativePath = await fileStorageService.SaveAsync(model.File, $"assets/{asset.AssetId}");

            var revision = new Revision
            {
                AssetId = asset.AssetId,
                FilePath = relativePath,
                FileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant(),
                OriginalFileName = Path.GetFileName(model.File.FileName),
                FileSizeBytes = model.File.Length,
                RevisionNumber = nextNumber,
                UploadedByUserId = userId,
                UploadedOn = DateTime.UtcNow,
                ChangeNotes = model.ChangeNotes
            };

            string? thumbnailPath = null;

            dbContext.Revisions.Add(revision);

            if (thumbnail != null)
            {
                try
                {
                    if (await fileStorageService.IsValidThumbnailAsync(thumbnail))
                    {
                        thumbnail.Content.Position = 0;

                        thumbnailPath = await fileStorageService.SaveAsync(thumbnail.Content, thumbnail.FileName, $"assets/{asset.AssetId}");

                        revision.ThumbnailPath = thumbnailPath;
                        revision.ThumbnailGeneratedOn = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    // Thumbnail failure must not interrupt the original revision upload.
                    logger.LogWarning(ex, "Failed to save thumbnail for revision {RevisionNumber} of asset {AssetId}", revision.RevisionNumber, asset.AssetId);
                }
            }

            asset.CurrentRevision = revision;
            asset.UpdatedOn = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                fileStorageService.DeleteFile(relativePath);

                if (thumbnailPath != null)
                {
                    fileStorageService.DeleteFile(thumbnailPath);
                }

                throw;
            }
        }

        public async Task<EditAssetViewModel?> GetEditFormAsync(int assetId, int userId)
        {
            var result = await dbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted)
                .Select(a => new EditAssetViewModel
                {
                    AssetId = a.AssetId,
                    AssetName = a.AssetName,
                    Description = a.Description,
                    MaterialId = a.MaterialId,
                    PartNumber = a.PartNumber,
                    ProjectId = a.ProjectId
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return null;
            }

            result.Materials = await dbContext.Materials
                .Select(m => new SelectListItem
                {
                    Value = m.MaterialId.ToString(),
                    Text = m.MaterialName
                })
                .ToListAsync();

            result.Projects = await dbContext.Projects
                .Where(p => p.OwnerId == userId && !p.IsDeleted)
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectId.ToString(),
                    Text = p.ProjectName
                })
                .ToListAsync();

            return result;
        }

        public async Task<bool> EditAsync(EditAssetViewModel model, int userId)
        {
            var asset = await dbContext.Assets
                .Where(a => a.AssetId == model.AssetId && a.OwnerId == userId && !a.IsDeleted)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return false;
            }

            bool projectExist = await dbContext.Projects
                .AnyAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == userId && !p.IsDeleted);

            if (!projectExist)
            {
                return false;
            }

            asset.PartNumber = model.PartNumber;
            asset.AssetName = model.AssetName;
            asset.Description = model.Description;
            asset.MaterialId = model.MaterialId;
            asset.ProjectId = model.ProjectId;
            asset.UpdatedOn = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SelectListItem>> GetMaterialsAsync()
        {
            var result = await dbContext.Materials
                .AsNoTracking()
                .Select(m => new SelectListItem
                {
                    Value = m.MaterialId.ToString(),
                    Text = m.MaterialName
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<SelectListItem>> GetProjectsAsync()
        {
            var result = await dbContext.Projects
                .AsNoTracking()
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectId.ToString(),
                    Text = p.ProjectName
                })
                .ToListAsync();

            return result;
        }

        public async Task<ReplaceAssetFileViewModel?> GetReplaceAssetFileViewModelAsync(int assetId, int userId)
        {
            var result = await dbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted)
                .Select(a => new ReplaceAssetFileViewModel
                {
                    AssetId = a.AssetId,
                    AssetName = a.AssetName
                })
                .FirstOrDefaultAsync();

            return result;
        }
        public async Task<bool> ReplaceAssetFileAsync(int assetId, int userId, ReplaceAssetFileViewModel model, ThumbnailUpload? thumbnail)
        {
            var asset = await dbContext.Assets
                .Include(a => a.Revisions)
                .FirstOrDefaultAsync(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted);

            if (asset == null)
            {
                return false;
            }

            var oldFilePaths = asset.Revisions.Select(r => r.FilePath).ToList();
            var oldThumbnailPaths = asset.Revisions.Select(r => r.ThumbnailPath).Where(p => !string.IsNullOrEmpty(p)).ToList();

            string newFilePath = await fileStorageService.SaveAsync(model.File, $"assets/{asset.AssetId}");

            string? thumbnailPath = null;

            try
            {

                if (thumbnail != null)
                {
                    try
                    {
                        if (await fileStorageService.IsValidThumbnailAsync(thumbnail))
                        {
                            thumbnail.Content.Position = 0;
                            thumbnailPath = await fileStorageService.SaveAsync(thumbnail.Content, thumbnail.FileName, $"assets/{asset.AssetId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Thumbnail failure must not interrupt the original revision upload.
                        logger.LogWarning(ex, "Failed to save thumbnail for replaced asset {AssetId}", asset.AssetId);
                    }
                }


                asset.CurrentRevisionId = null;
                await dbContext.SaveChangesAsync();

                dbContext.Revisions.RemoveRange(asset.Revisions);

                var revision = new Revision
                {
                    AssetId = asset.AssetId,
                    FilePath = newFilePath,
                    FileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant(),
                    OriginalFileName = Path.GetFileName(model.File.FileName),
                    FileSizeBytes = model.File.Length,
                    RevisionNumber = 1,
                    UploadedByUserId = userId,
                    UploadedOn = DateTime.UtcNow,
                    ChangeNotes = "Initial revision [File Replaced]",
                    ThumbnailPath = thumbnailPath,
                    ThumbnailGeneratedOn = thumbnailPath != null ? DateTime.UtcNow : null
                };

                dbContext.Revisions.Add(revision);
                asset.CurrentRevision = revision;
                asset.UpdatedOn = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();
            }
            catch
            {
                fileStorageService.DeleteFile(newFilePath);

                if (thumbnailPath != null)
                {
                    fileStorageService.DeleteFile(thumbnailPath);
                }

                throw;
            }

            foreach (var oldFilePath in oldFilePaths)
            {
                fileStorageService.DeleteFile(oldFilePath);
            }

            foreach (var oldThumbnailPath in oldThumbnailPaths)
            {
                fileStorageService.DeleteFile(oldThumbnailPath);
            }

            return true;
        }

        public async Task<bool> DeleteAssetPermanently(int id, int userId)
        {
            var asset = await dbContext.Assets
                .Include(a => a.Revisions)
                .FirstOrDefaultAsync(a => a.AssetId == id && a.OwnerId == userId && !a.IsDeleted);

            if (asset == null)
            {
                return false;
            }

            var filePaths = asset.Revisions
                .Select(r => r.FilePath)
                .ToList();

            var thumbnailPaths = asset.Revisions
                .Where(r => !string.IsNullOrEmpty(r.ThumbnailPath))
                .Select(r => r.ThumbnailPath)
                .ToList();

            asset.CurrentRevisionId = null;
            await dbContext.SaveChangesAsync();

            dbContext.Revisions.RemoveRange(asset.Revisions);
            dbContext.Assets.Remove(asset);
            await dbContext.SaveChangesAsync();

            foreach (var path in filePaths)
            {
                fileStorageService.DeleteFile(path);
            }

            foreach (var path in thumbnailPaths)
            {
                fileStorageService.DeleteFile(path);
            }

            return true;
        }

        // Method
        private async Task<int> GetNextRevisionNumberAsync(int assetId)
        {
            int? max = await dbContext.Revisions
                .Where(r => r.AssetId == assetId)
                .MaxAsync(r => (int?)r.RevisionNumber);

            return (max ?? 0) + 1;
        }
    }
}
