
namespace PartManagementSystem.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels.Asset;

    using System.Security.Claims;

    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService projectService;
        public ProjectController(IProjectService projectService)
        {
            this.projectService = projectService;
        }

        // URL: ~/Project/Upload
        // Returns a view of a list form where User can create/upload an Asset and Project.

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var viewModel = await projectService.GetAssetUploadViewModelAsync();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(AssetUploadViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                int projectId = await projectService.ProcessAssetUploadViewModelAsync(viewModel, userId.Value);

                return RedirectToAction("Details", "Project", new { id = projectId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(viewModel.ProjectId.ToString(), "An error occurred saving the asset");

                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var projectName = await projectService.GetProjectNameAsync(id, userId.Value);

            if (projectName == null)
            {
                return NotFound();
            }

            var viewModel = await projectService.AddNewAssetUploadViewModelAsync(id, projectName);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddAssetViewModel model)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var projectName = await projectService.GetProjectNameAsync(model.ProjectId, userId.Value);

            if (projectName == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var reloadedModel = await projectService.AddNewAssetUploadViewModelAsync(model.ProjectId, projectName);

                model.Materials = reloadedModel.Materials;
                model.ProjectName = projectName;

                return View(model);
            }

            try
            {
                await projectService.ProcessAssetUploadViewModelAsync(model, userId.Value);
                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(model.ProjectId.ToString(), "An error occurred while saving the asset. Please try again.");

                var reloadedViewModel = await projectService.AddNewAssetUploadViewModelAsync(model.ProjectId, projectName);
                model.Materials = reloadedViewModel.Materials;
                model.ProjectName = projectName;

                return View(model);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {

            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var project = await projectService.GetDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // Methods
        private int? GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : null;
        }
    }
}
