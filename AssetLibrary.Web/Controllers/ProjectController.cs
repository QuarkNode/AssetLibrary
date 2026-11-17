namespace AssetLibrary.Controllers
{
    using AssetLibrary.Data.Exceptions;
    using AssetLibrary.Extensions;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Project;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService projectService;
        private readonly IThumbnailService thumbnailService;
        public ProjectController(IProjectService projectService, IThumbnailService thumbnailService)
        {
            this.projectService = projectService;
            this.thumbnailService = thumbnailService;
        }


        [HttpGet]
        public async Task<IActionResult> GetProjectModelPaths(int id)
        {
            var modelPaths = await projectService.GetProjectModelPathsAsync(
                id,
                User.GetRequiredUserId());

            if (modelPaths == null)
            {
                return NotFound();
            }

            return Json(modelPaths);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProjectThumbnail(int projectId, IFormFile? thumbnail)
        {
            var modelPaths = await projectService.GetProjectModelPathsAsync(
                projectId,
                User.GetRequiredUserId());

            if (modelPaths == null)
            {
                return NotFound();
            }

            if (modelPaths.Count == 0)
            {
                await thumbnailService.ClearProjectThumbnailAsync(projectId);
                return Ok();
            }

            if (thumbnail == null || thumbnail.Length == 0)
            {
                return BadRequest("A thumbnail is required.");
            }

            using var stream = thumbnail.OpenReadStream();

            var upload = new ThumbnailUpload
            {
                Content = stream,
                FileName = thumbnail.FileName,
                Length = thumbnail.Length
            };

            await thumbnailService.SaveProjectThumbnailAsync(projectId, upload);

            return Ok();
        }

        // URL: ~/Project/Details/id
        // Returns a view of a Project with its information.

        [HttpGet]
        public async Task<IActionResult> Details(int id, bool updateThumbnail = false)
        {
            var project = await projectService.GetDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.UpdateThumbnail = updateThumbnail;

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

            TempData["SuccessMessage"] = "Project has been deleted successfully.";
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadAssetViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            using Stream? thumbnailStream = viewModel.Thumbnail?.OpenReadStream();

            ThumbnailUpload? thumbnail = null;

            if (thumbnailStream != null && viewModel.Thumbnail!.Length > 0)
            {
                thumbnail = new ThumbnailUpload
                {
                    Content = thumbnailStream,
                    FileName = viewModel.Thumbnail.FileName,
                    Length = viewModel.Thumbnail.Length
                };
            }

            try
            {
                int projectId = await projectService.ProcessAssetUploadViewModelAsync(viewModel, thumbnail, User.GetRequiredUserId());

                return RedirectToAction("Details", "Project", new { id = projectId, updateThumbnail = true });
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
                using Stream? thumbnailStream = model.Thumbnail?.OpenReadStream();

                ThumbnailUpload? thumbnail = null;

                if (thumbnailStream != null && model.Thumbnail!.Length > 0)
                {
                    thumbnail = new ThumbnailUpload
                    {
                        Content = thumbnailStream,
                        FileName = model.Thumbnail.FileName,
                        Length = model.Thumbnail.Length
                    };
                }

                await projectService.ProcessAssetUploadViewModelAsync(model, thumbnail, User.GetRequiredUserId());
                return RedirectToAction("Details", "Project", new { id = model.ProjectId, updateThumbnail = true });
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
