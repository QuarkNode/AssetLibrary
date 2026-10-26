namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Home;
    using AssetLibrary.Services.Models.Project;
    using Microsoft.EntityFrameworkCore;

    public class HomeService : IHomeService
    {
        private readonly AssetLibraryDbContext dbContext;

        public HomeService(AssetLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<HomeViewModel> GetHomeViewModelAsync(int? userId)
        {
            var viewModel = new HomeViewModel();

            if (!userId.HasValue)
            {
                return viewModel;
            }

            viewModel.IsLoggedIn = true;

            viewModel.Projects = await dbContext.Projects
                .AsNoTracking()
                .Where(p => p.OwnerId == userId.Value && !p.IsDeleted)
                .OrderBy(p => p.ProjectName)
                .Select(p => new ProjectCardViewModel
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    AssetCount = p.Assets.Count(a => !a.IsDeleted)
                })
                .ToListAsync();

            viewModel.ProjectCount = viewModel.Projects.Count;

            return viewModel;
        }
    }
}