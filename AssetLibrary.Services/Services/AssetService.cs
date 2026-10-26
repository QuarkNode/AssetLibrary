namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Data.Models;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Revision;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    public class AssetService : IAssetService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IFileStorageService fileStorageService;
        public AssetService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService)
        {
            this.dbContext = dbContext;
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
                    OriginalFIleName = r.OriginalFileName
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
                asset.CurrentRevisionId = null;

                //asset.CurrentRevisionId = await dbContext.Revisions
                //    .Where(r => r.AssetId == asset.AssetId && r.RevisionId != revisionId)
                //    .OrderByDescending(r => r.RevisionNumber)
                //    .Select(r => (int?)r.RevisionId)
                //    .FirstOrDefaultAsync();

                await dbContext.SaveChangesAsync();
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

        public async Task<bool> CreateRevisionAsync(AddRevisionViewModel model, int userId)
        {
            //var asset = await CreateAsset(model, userId);

            //if (asset == null)
            //{
            //    return false;
            //}

            //string subFolder = Path.Combine("assets", asset.AssetId.ToString()); // <---
            //string relativePath = await fileStorageService.SaveAsync(model.File, subFolder); // <--- 
            //string extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();

            //Revision revision = new()
            //{
            //    AssetId = asset.AssetId,
            //    FilePath = relativePath,
            //    FileExtension = extension,
            //    OriginalFileName = Path.GetFileName(model.File.FileName),
            //    FileSizeBytes = model.File.Length,
            //    RevisionNumber = model.NextRevisionNumber,
            //    UploadedByUserId = userId,
            //    UploadedOn = DateTime.UtcNow,
            //    ChangeNotes = model.ChangeNotes
            //};

            //dbContext.Revisions.Add(revision);
            //asset.CurrentRevision = revision;
            //asset.UpdatedOn = DateTime.UtcNow;
            //try
            //{
            //    await dbContext.SaveChangesAsync();
            //    return true;
            //}
            //catch
            //{
            //    fileStorageService.DeleteFile(relativePath);
            //    throw;
            //}

            var asset = await dbContext.Assets
                .FirstOrDefaultAsync(a => a.AssetId == model.AssetId && a.OwnerId == userId && !a.IsDeleted);

            if (asset == null)
            {
                return false;
            }

            // Don't trust the number from the form; compute it here
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

            dbContext.Revisions.Add(revision);
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
        public async Task<bool> ReplaceAssetFileAsync(int assetId, int userId, ReplaceAssetFileViewModel model)
        {
            var asset = await dbContext.Assets
                .Include(a => a.Revisions)
                .FirstOrDefaultAsync(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted);

            if (asset == null)
            {
                return false;
            }

            var oldFilePaths = asset.Revisions.Select(r => r.FilePath).ToList();
            string newFilePath = await fileStorageService.SaveAsync(model.File, $"assets/{asset.AssetId}");

            try
            {
                // Clear the pointer first so the old revisions can be removed
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
                    ChangeNotes = "Initial revision [File Replaced]"
                };

                dbContext.Revisions.Add(revision);
                asset.CurrentRevision = revision;
                asset.UpdatedOn = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();
            }
            catch
            {
                fileStorageService.DeleteFile(newFilePath);
                throw;
            }

            foreach (var path in oldFilePaths)
            {
                fileStorageService.DeleteFile(path);
            }

            return true;
        }

        public async Task<bool> DeleteAssetPermanently(Asset asset)
        {
            if (asset == null)
            {
                return false;
            }

            var tracked = await dbContext.Assets
                .Include(a => a.Revisions)
                .FirstOrDefaultAsync(a => a.AssetId == asset.AssetId && a.OwnerId == asset.OwnerId);

            if (tracked == null)
            {
                return false;
            }

            var filePaths = tracked.Revisions.Select(r => r.FilePath).ToList();

            tracked.CurrentRevisionId = null;
            await dbContext.SaveChangesAsync();

            dbContext.Revisions.RemoveRange(tracked.Revisions);
            dbContext.Assets.Remove(tracked);
            await dbContext.SaveChangesAsync();

            foreach (var path in filePaths)
            {
                //if (!string.IsNullOrEmpty(path) && File.Exists(path))
                //{
                //    File.Delete(path);
                //}

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
