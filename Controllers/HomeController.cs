
namespace PartManagementSystem.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels.Home;
    using System.Security.Claims;

    public class HomeController : Controller
    {
        private readonly IProjectService projectService;
        public HomeController(IProjectService projectService)
        {
            this.projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            int? userId = GetUserId();

            if (userId.HasValue)
            {
                viewModel.IsLoggedIn = true;

                viewModel.Projects = (await projectService.GetUserProjectsAsync(userId.Value)).ToList();

                viewModel.ProjectCount = viewModel.Projects.Count;
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        private int? GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : null;
        }
    }

}
