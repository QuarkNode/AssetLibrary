namespace AssetLibrary.Controllers
{
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Project;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService projectService;
        public ProjectController(IProjectService projectService)
        {
            this.projectService = projectService;
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

        // Soft deletes a Project. Preserves the Project in DB for accidental deletion or unsure confirmations.
        [HttpPost]
        public async Task<IActionResult> Delete(int id) // id = ProjectId
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Challenge();
            }

            bool deleted = await projectService.DeleteProjectAsync(id, userId.Value);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Home");
        }

        // URL: ~/Project/Edit/id
        [HttpGet]
        public async Task<IActionResult> Edit(int id) // id = ProjectId
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Challenge();
            }

            var model = await projectService.GetEditFormAsync(id, userId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            int? userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool updated = await projectService.EditAsync(model, userId.Value);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.ProjectId });
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
        public async Task<IActionResult> Upload(UploadAssetViewModel viewModel)
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
                ModelState.AddModelError(string.Empty, "An error occurred saving the asset");

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
                ModelState.AddModelError(string.Empty, "An error occurred while saving the asset. Please try again.");

                var reloadedViewModel = await projectService.AddNewAssetUploadViewModelAsync(model.ProjectId, projectName);
                model.Materials = reloadedViewModel.Materials;
                model.ProjectName = projectName;

                return View(model);
            }

        }


        // Methods
        private int? GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : null;
        }
    }
}
