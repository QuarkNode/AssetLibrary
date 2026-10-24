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

        public async Task<RevisionFileDto?> GetRevisionFileAsync(int revisionId, int userId)
        {
            var revision = await dbContext.Revisions
                .AsNoTracking()
                .Where(r => r.RevisionId == revisionId
                            && r.Asset.OwnerId == userId
                            && !r.Asset.IsDeleted)
                .Select(r => new RevisionFileDto
                {
                    FilePath = r.FilePath,
                    OriginalFIleName = r.OriginalFileName
                })
                .FirstOrDefaultAsync();

            return revision;
        }

        public async Task<AddRevisionViewModel?> GetAddRevisionViewModelAsync(int assetId, int userId)
        {
            var asset = await dbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted)
                .Select(a => new
                {
                    a.ProjectId,
                    a.AssetName,
                    NextRevisionNumber = a.Revisions.Count() + 1
                })
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return null;
            }

            return new AddRevisionViewModel
            {
                AssetId = assetId,
                ProjectId = asset.ProjectId,
                AssetName = asset.AssetName,
                NextRevisionNumber = asset.NextRevisionNumber
            };
        }

        public async Task<bool> PopulateRevisionMetadataAsync(AddRevisionViewModel model, int userId)
        {
            var asset = await dbContext.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == model.AssetId && a.OwnerId == userId && !a.IsDeleted)
                .Select(a => new { a.AssetName, a.ProjectId })
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return false;
            }

            model.AssetName = asset.AssetName;
            model.ProjectId = asset.ProjectId;
            model.NextRevisionNumber = await GetNextRevisionNumberAsync(model.AssetId);

            return true;
        }

        public async Task<bool> CreateRevisionAsync(AddRevisionViewModel model, int userId)
        {
            var asset = await CreateAsset(model, userId);

            if (asset == null)
            {
                return false;
            }

            string subFolder = Path.Combine("assets", asset.AssetId.ToString());
            string relativePath = await fileStorageService.SaveAsync(model.File, subFolder);
            string extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();

            Revision revision = new()
            {
                AssetId = asset.AssetId,
                FilePath = relativePath, // Provided directly by your service
                FileExtension = extension,
                OriginalFileName = Path.GetFileName(model.File.FileName),
                FileSizeBytes = model.File.Length,
                RevisionNumber = model.NextRevisionNumber,
                UploadedByUserId = userId,
                UploadedOn = DateTime.UtcNow,
                ChangeNotes = model.ChangeNotes
            };

            dbContext.Revisions.Add(revision);
            asset.CurrentRevision = revision;
            asset.UpdatedOn = DateTime.UtcNow;

            // 3. Save to DB with File Rollback on Error
            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Roll back file upload if database save fails
                await fileStorageService.DeleteFile(relativePath);
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

            bool targetProjectExists = await dbContext.Projects
                .AnyAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == userId && !p.IsDeleted);
            if (!targetProjectExists)
            {
                return false;
            }

            asset.PartNumber = model.PartNumber;
            asset.AssetName = model.AssetName;
            asset.Description = model.Description;
            asset.MaterialId = model.MaterialId;
            asset.ProjectId = model.ProjectId;

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
        public async Task<bool> ReplaceCurrentRevisionFileAsync(int assetId, int userId, ReplaceAssetFileViewModel model)
        {
            var asset = await dbContext.Assets
                .Include(a => a.CurrentRevision)
                .FirstOrDefaultAsync(a => a.AssetId == assetId && a.OwnerId == userId && !a.IsDeleted);

            if (asset == null && asset.CurrentRevision == null)
            {
                return false;
            }

            string oldFilePath = asset.CurrentRevision.FilePath;
            string newFilePath;

            try
            {
                string subFolder = $"assets/{asset.AssetId}";
                newFilePath = await fileStorageService.SaveAsync(model.File, subFolder);
            }
            catch
            {

                throw;
            }

            var revision = asset.CurrentRevision;
            revision.FilePath = newFilePath;
            revision.FileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            revision.OriginalFileName = model.File.FileName;
            revision.FileSizeBytes = model.File.Length;

            asset.UpdatedOn = DateTime.UtcNow;

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
            revision.ChangeNotes = string.IsNullOrWhiteSpace(revision.ChangeNotes)
                ? $"File repalced {timestamp}"
                : $"{revision.ChangeNotes}\nFile replaced on {timestamp}";

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch
            {
                await fileStorageService.DeleteFile(newFilePath);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldFilePath))
            {
                await fileStorageService.DeleteFile(oldFilePath);
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
        private async Task<Asset?> CreateAsset(AddRevisionViewModel model, int userId)
        {
            return await dbContext.Assets
                            .FirstOrDefaultAsync(a => a.AssetId == model.AssetId && a.OwnerId == userId && !a.IsDeleted);
        }


    }
}
