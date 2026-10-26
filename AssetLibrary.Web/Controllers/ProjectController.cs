namespace AssetLibrary.Controllers
{
    using AssetLibrary.Data.Exceptions;
    using AssetLibrary.Extensions;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Project;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService projectService;
        public ProjectController(IProjectService projectService)
        {
            this.projectService = projectService;
        }


        // URL: ~/Project/Details/id
        // Returns a view of a Project with its information.

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await projectService.GetDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // URL: ~/Project/Delete/id
        // Soft deletes a Project. Preserves the Project in DB for accidental deletion or unsure confirmations.

        [HttpPost]
        public async Task<IActionResult> Delete(int id) // id = ProjectId
        {
            bool deleted = await projectService.DeleteProjectAsync(id, User.GetRequiredUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Home");
        }

        // URL: ~/Project/Edit/id
        // Returns a view of a form to edit a Project's information.

        [HttpGet]
        public async Task<IActionResult> Edit(int id) // id = ProjectId
        {
            var model = await projectService.GetEditFormAsync(id, User.GetRequiredUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool updated = await projectService.EditAsync(model, User.GetRequiredUserId());

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

            try
            {
                int projectId = await projectService.ProcessAssetUploadViewModelAsync(viewModel, User.GetRequiredUserId());

                return RedirectToAction("Details", "Project", new { id = projectId });
            }
            catch (ProjectLimitExceededException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(viewModel);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occured saving the asset.");

                return View(viewModel);
            }
        }

        // URL: ~/Project/Add/{ProjectId}
        // Returns a view of a form where User can upload an Asset to an existing Project.

        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {

            var projectName = await projectService.GetProjectNameAsync(id, User.GetRequiredUserId());

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
            var projectName = await projectService.GetProjectNameAsync(model.ProjectId, User.GetRequiredUserId());

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
                await projectService.ProcessAssetUploadViewModelAsync(model, User.GetRequiredUserId());
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
    }
}
