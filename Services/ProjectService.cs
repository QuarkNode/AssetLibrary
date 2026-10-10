
namespace PartManagementSystem.Services
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using PartManagementSystem.Data;
    using PartManagementSystem.Data.Models;
    using PartManagementSystem.Data.Models.Enum;
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels.Asset;

    public class ProjectService : IProjectService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IFileStorageService fileStorageService;

        public ProjectService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
        }

        public async Task<Project?> GetDetailsAsync(int projectId)
        {
            var project = await dbContext.Projects
                .Include(p => p.Owner)
                .Include(p => p.Assets)
                    .ThenInclude(a => a.Material)
                .Include(p => p.Assets)
                    .ThenInclude(a => a.CurrentRevision)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            return project;
        }

        public async Task<AssetUploadViewModel> GetAssetUploadViewModelAsync()
        {
            var viewModel = new AssetUploadViewModel
            {
                Projects = await dbContext.Projects
                .AsNoTracking()
                .Select(p => new SelectListItem { Value = p.ProjectId.ToString(), Text = p.ProjectName })
                .ToListAsync(),

                Materials = await dbContext.Materials
                .AsNoTracking()
                .Select(m => new SelectListItem { Value = m.MaterialId.ToString(), Text = m.MaterialName })
                .ToListAsync()
            };

            return viewModel;
        }

        public async Task<AddAssetViewModel> AddNewAssetUploadViewModelAsync(int routeId, string projectName)
        {
            var viewModel = new AddAssetViewModel
            {
                ProjectId = routeId,
                ProjectName = projectName,
                Materials = await dbContext.Materials
                    .AsNoTracking()
                    .Select(m => new SelectListItem { Value = m.MaterialId.ToString(), Text = m.MaterialName })
                    .ToListAsync()
            };

            return viewModel;
        }

        public async Task ProcessAssetUploadViewModelAsync(AddAssetViewModel model, int userId)
        {
            Asset asset = new Asset
            {
                ProjectId = model.ProjectId,
                PartNumber = model.PartNumber,
                AssetName = model.AssetName,
                Description = model.Description,
                MaterialId = model.MaterialId,
                Status = Status.Draft,
                CurrentRevisionId = null,
                OwnerId = userId,
                CreatedOn = DateTime.UtcNow
            };

            dbContext.Assets.Add(asset);
            await dbContext.SaveChangesAsync();

            string subFolder = $"assets/{asset.AssetId}";
            string relativeFilePath = await fileStorageService.SaveAsync(model.File, subFolder);
            string extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();

            Revision revision = new Revision
            {
                AssetId = asset.AssetId,
                FilePath = relativeFilePath,
                FileExtension = extension,
                FileSizeBytes = model.File.Length,
                OriginalFileName = Path.GetFileName(model.File.FileName),
                RevisionNumber = 1,
                UploadedByUserId = userId,
                UploadedOn = DateTime.UtcNow,
                ChangeNotes = "Initial upload"
            };

            dbContext.Revisions.Add(revision);
            await dbContext.SaveChangesAsync();

            asset.CurrentRevisionId = revision.RevisionId;
            await dbContext.SaveChangesAsync();
        }
        public async Task<int> ProcessAssetUploadViewModelAsync(AssetUploadViewModel viewModel, int userId)
        {
            int projectId;

            if (viewModel.IsNewProject)
            {
                Project newProject = CreateNewProject(viewModel, userId);

                dbContext.Projects.Add(newProject);
                await dbContext.SaveChangesAsync();

                projectId = newProject.ProjectId;
            }
            else
            {
                projectId = viewModel.ProjectId!.Value;
            }

            Asset newAsset = CreateNewAsset(viewModel, userId, projectId);

            dbContext.Assets.Add(newAsset);
            await dbContext.SaveChangesAsync();

            // File Creation

            string subFolder = $"assets/{newAsset.AssetId}";
            string relativeFilePath = await fileStorageService.SaveAsync(viewModel.File, subFolder);
            string extension = Path.GetExtension(viewModel.File.FileName).ToLowerInvariant();

            // File Creation

            Revision revision = CreateNewRevision(viewModel, userId, newAsset, relativeFilePath, extension);

            dbContext.Revisions.Add(revision);
            await dbContext.SaveChangesAsync();

            newAsset.CurrentRevisionId = revision.RevisionId;
            await dbContext.SaveChangesAsync();

            return newAsset.ProjectId;
        }

        public async Task<string?> GetProjectNameAsync(int routeProjectId, int userId)
        {
            string? projectName = await dbContext.Projects
                .AsNoTracking()
                .Where(p => p.ProjectId == routeProjectId && p.OwnerId == userId && !p.IsDeleted)
                .Select(p => p.ProjectName)
                .FirstOrDefaultAsync();

            return projectName;
        }


        // Methods
        private static Project CreateNewProject(AssetUploadViewModel viewModel, int userId)
        {
            return new()
            {
                ProjectName = viewModel.NewProjectName!,
                Description = viewModel.NewProjectDescription,
                OwnerId = userId,
                Status = Status.Draft,
                CreatedOn = DateTime.UtcNow
            };
        }
        private static Asset CreateNewAsset(AssetUploadViewModel viewModel, int userId, int projectId)
        {
            return new Asset
            {
                ProjectId = projectId,
                PartNumber = viewModel.PartNumber,
                AssetName = viewModel.AssetName,
                Description = viewModel.Description,
                MaterialId = viewModel.MaterialId,
                Status = Status.Draft,
                CurrentRevisionId = null,
                OwnerId = userId,
                CreatedOn = DateTime.UtcNow

            };
        }
        private static Revision CreateNewRevision(AssetUploadViewModel viewModel, int userId, Asset newAsset, string relativeFilePath, string extension)
        {
            return new()
            {
                AssetId = newAsset.AssetId,
                FilePath = relativeFilePath,
                FileExtension = extension,
                FileSizeBytes = viewModel.File.Length,
                OriginalFileName = Path.GetFileName(viewModel.File.FileName),
                RevisionNumber = 1,
                UploadedByUserId = userId,
                UploadedOn = DateTime.UtcNow,
                ChangeNotes = "Initial upload"
            };
        }
    }
}
