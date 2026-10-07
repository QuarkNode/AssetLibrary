
namespace PartManagementSystem.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using PartManagementSystem.Data;
    using PartManagementSystem.ViewModels.Home;
    using PartManagementSystem.ViewModels.Project;

    using System.Security.Claims;

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public HomeController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = GetUserId();

            var viewModel = new HomeViewModel();

            if (userId.HasValue)
            {
                viewModel.IsLoggedIn = true;

                viewModel.Projects = await dbContext.Projects
                    .AsNoTracking()
                    .Where(p => p.OwnerId == userId && !p.IsDeleted)
                    .OrderBy(p => p.ProjectName)
                    .Select(p => new ProjectCardViewModel
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        AssetCount = p.Assets.Count(a => !a.IsDeleted)
                    })
                    .ToListAsync();

                viewModel.ProjectCount = viewModel.Projects.Count();
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }

}
