namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Trash;
    using Microsoft.EntityFrameworkCore;

    public class TrashService : ITrashService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IFileStorageService fileStorageService;
        public TrashService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
        }
        public async Task<DeletedProjectDetailsViewModel?> GetDeletedProjectDetailsAsync(int projectId)
        {
            var project = await dbContext.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Assets)
                    .ThenInclude(r => r.Revisions)
                .Include(p => p.Assets)
                    .ThenInclude(m => m.Material)
                .Include(p => p.Assets)
                    .ThenInclude(a => a.CurrentRevision)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.IsDeleted);

            if (project == null)
            {
                return null;
            }

            var model = new DeletedProjectDetailsViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                OwnerFirstName = project.Owner.FirstName,
                OwnerLastName = project.Owner.LastName,
                Status = project.Status,
                CreatedOn = project.CreatedOn,
                DeletedOn = project.DeletedOn,
                Assets = project.Assets.Select(a => new AssetCardViewModel
                {
                    AssetId = a.AssetId,
                    AssetName = a.AssetName,
                    Status = a.Status,
                    IsDeleted = true,
                    CurrentRevisionNumber = a.CurrentRevision?.RevisionNumber.ToString(),
                    RevisionCount = a.Revisions?.Count ?? 0,
                    PartNumber = a.PartNumber,
                    MaterialName = a.Material?.MaterialName
                })
                .ToList()
            };

            return model;
        }

        public async Task<List<DeletedProjectViewModel>> GetDeletedProjectsViewModelAsync(int userId)
        {
            var result = await dbContext.Projects
                .AsNoTracking()
                .Where(p => p.OwnerId == userId && p.IsDeleted)
                .Select(p => new DeletedProjectViewModel
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    ThumbnailUrl = null,
                    AssetCount = p.Assets.Count
                })
                .ToListAsync();

            return result;
        }

        public async Task<bool> RestoreProjectAsync(int projectId, int userId)
        {
            var project = await dbContext.Projects
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(P => P.ProjectId == projectId && P.OwnerId == userId && P.IsDeleted);

            if (project == null)
            {
                return false;
            }

            project.IsDeleted = false;
            project.DeletedOn = null;

            foreach (var asset in project.Assets)
            {
                asset.IsDeleted = false;
                asset.DeletedOn = null;
            }

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePermanentlyAsync(int projectId, int userId)
        {
            var project = await dbContext.Projects
                .Include(p => p.Assets)
                    .ThenInclude(a => a.Revisions)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.OwnerId == userId && p.IsDeleted);


            if (project == null)
            {
                return false;
            }

            var assets = project.Assets;
            var revisions = assets.SelectMany(a => a.Revisions).ToList();

            var filePaths = assets
                .SelectMany(a => a.Revisions)
                .Select(r => r.FilePath)
                .ToList();

            foreach (var asset in assets)
            {
                asset.CurrentRevisionId = null;
            }

            await dbContext.SaveChangesAsync();

            dbContext.Revisions.RemoveRange(revisions);
            dbContext.Assets.RemoveRange(assets);
            dbContext.Projects.Remove(project);

            await dbContext.SaveChangesAsync();

            foreach (var path in filePaths)
            {
                await fileStorageService.DeleteFile(path);
            }

            return true;
        }
    }
}
