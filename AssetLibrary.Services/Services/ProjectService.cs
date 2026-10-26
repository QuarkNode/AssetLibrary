namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Data.Exceptions;
    using AssetLibrary.Data.Models;
    using AssetLibrary.Data.Models.Enum;

    using AssetLibrary.Services.Contracts;

    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Project;

    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    using static AssetLibrary.Data.Common.ViewModelValidation;

    public class ProjectService : IProjectService
    {
        private readonly AssetLibraryDbContext dbContext;
        private readonly IFileStorageService fileStorageService;

        public ProjectService(AssetLibraryDbContext dbContext, IFileStorageService fileStorageService)
        {
            this.dbContext = dbContext;
            this.fileStorageService = fileStorageService;
        }

        public async Task<ProjectDetailsViewModel?> GetDetailsAsync(int projectId)
        {
            var projectViewModel = await dbContext.Projects
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .Select(p => new ProjectDetailsViewModel
                {
                    ProjectId = p.ProjectId,
                    OwnerId = p.OwnerId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    OwnerFirstName = p.Owner.FirstName,
                    OwnerLastName = p.Owner.LastName,
                    CreatedOn = p.CreatedOn,
                    Status = p.Status,
                    Assets = p.Assets
                        .Where(a => !a.IsDeleted)
                        .Select(a => new AssetCardViewModel
                        {
                            AssetId = a.AssetId,
                            PartNumber = a.PartNumber,
                            AssetName = a.AssetName,
                            Status = a.Status,
                            MaterialName = a.Material.MaterialName,
                            CurrentRevisionNumber = a.CurrentRevision.RevisionNumber,
                            IsDeleted = false
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return projectViewModel;
        }

        public async Task<UploadAssetViewModel> GetAssetUploadViewModelAsync()
        {
            var viewModel = new UploadAssetViewModel
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
            //int count = await dbContext.Projects
            //    .AsNoTracking()
            //    .CountAsync(P => P.OwnerId == userId && !P.IsDeleted);


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
        public async Task<int> ProcessAssetUploadViewModelAsync(UploadAssetViewModel viewModel, int userId)
        {
            int count = await dbContext.Projects
                .CountAsync(p => p.OwnerId == userId);

            if (count >= ProjectsMaxLength)
            {
                throw new ProjectLimitExceededException(ProjectsMaxLength);
            }

            int projectId;

            if (viewModel.IsNewProject)
            {
                if (count >= ProjectsMaxLength)
                {
                    throw new ProjectLimitExceededException(ProjectsMaxLength);
                }

                Project newProject = CreateNewProject(viewModel, userId);

                dbContext.Projects.Add(newProject);
                await dbContext.SaveChangesAsync();

                projectId = newProject.ProjectId;
            }
            else
            {
                bool owns = await dbContext.Projects
                    .AnyAsync(p => p.ProjectId == viewModel.ProjectId && p.OwnerId == userId);

                if (!owns)
                {
                    throw new InvalidOperationException("Project not found.");
                }

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

        public async Task<bool> DeleteProjectAsync(int projectId, int userId)
        {
            var project = await dbContext.Projects
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId
                                       && p.OwnerId == userId
                                       && !p.IsDeleted);

            if (project == null)
            {
                return false;
            }

            DateTime now = DateTime.UtcNow;

            project.IsDeleted = true;
            project.DeletedOn = now;

            foreach (var asset in project.Assets.Where(a => !a.IsDeleted))
            {
                asset.IsDeleted = true;
                asset.DeletedOn = now;
            }
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<EditProjectViewModel?> GetEditFormAsync(int projectId, int userId)
        {
            var result = await dbContext.Projects
                .AsNoTracking()
                .Where(p => p.ProjectId == projectId && p.OwnerId == userId && !p.IsDeleted)
                .Select(p => new EditProjectViewModel
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<bool> EditAsync(EditProjectViewModel model, int userId)
        {
            var project = await dbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId
                                       && p.OwnerId == userId
                                       && !p.IsDeleted);

            if (project == null)
            {
                return false;
            }

            project.ProjectName = model.ProjectName;
            project.Description = model.Description;
            project.UpdatedOn = DateTime.UtcNow;
            project.Status = model.Status;

            await dbContext.SaveChangesAsync();

            return true;
        }

        // Methods
        private static Project CreateNewProject(UploadAssetViewModel viewModel, int userId)
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
        private static Asset CreateNewAsset(UploadAssetViewModel viewModel, int userId, int projectId)
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
        private static Revision CreateNewRevision(UploadAssetViewModel viewModel, int userId, Asset newAsset, string relativeFilePath, string extension)
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
