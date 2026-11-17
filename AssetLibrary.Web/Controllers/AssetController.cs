using AssetLibrary.Extensions;
using AssetLibrary.Services.Contracts;
using AssetLibrary.Services.Models;
using AssetLibrary.Services.Models.Asset;
using AssetLibrary.Services.Models.Revision;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetLibrary.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly IAssetService assetService;
        private readonly IFileStorageService fileStorage;
        private readonly IThumbnailService thumbnailService;
        public AssetController(IAssetService assetService, IFileStorageService fileStorage, IThumbnailService thumbnailService)
        {
            this.assetService = assetService;
            this.fileStorage = fileStorage;
            this.thumbnailService = thumbnailService;
        }


        // URL: ~/Project/Details/id
        // Returns a view of an Asset with its information.

        [HttpGet]
        public async Task<IActionResult> Details(int id) // id = Asset Id
        {

            var asset = await assetService.GetAssetDetailsAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }


        // URL: ~/Asset/Details/id
        // Returns an action to download an Asset file based on Revision id.

        [HttpGet]
        public async Task<IActionResult> DownloadRevision(int id) // id = Revision Id
        {
            var revision = await assetService.GetRevisionFileAsync(id, User.GetRequiredUserId());

            if (revision == null)
            {
                return NotFound();
            }

            string? fullPath = fileStorage.GetPhysicalPath(revision.FilePath);

            if (fullPath == null)
            {
                return NotFound();
            }

            return PhysicalFile(fullPath, "application/octet-stream", revision.OriginalFIleName);
        }

        public async Task<IActionResult> GetModelPreview(int id)
        {
            var revision = await assetService.GetCurrentRevisionFileAsync(id, User.GetRequiredUserId());

            if (revision == null)
            {
                return NotFound();
            }

            var allowedExtensions = new[] { ".obj", ".stl", ".fbx" };

            if (!allowedExtensions.Contains(revision.FileExtension.ToLowerInvariant()))
            {
                return BadRequest("Unsupported 3D model format.");
            }

            string? fullPath = fileStorage.GetPhysicalPath(revision.FilePath);

            if (fullPath == null)
            {
                return NotFound();
            }

            return Json(new
            {
                revisionId = revision.RevisionId,
                fileName = revision.OriginalFIleName,
                extension = revision.FileExtension,
                url = Url.Action(nameof(PreviewModelFile),
                new { id = revision.RevisionId })
            });
        }

        // Serves the 3D model file to the viewer.

        [HttpGet]
        public async Task<IActionResult> PreviewModelFile(int id) // id = Revision Id
        {
            var revision = await assetService.GetRevisionFileAsync(
                id, User.GetRequiredUserId());

            if (revision == null)
            {
                return NotFound();
            }

            string? fullPath = fileStorage.GetPhysicalPath(revision.FilePath);

            if (fullPath == null || !System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            // Serve the file inline, without forcing a download.
            return PhysicalFile(
                fullPath,
                "application/octet-stream",
                enableRangeProcessing: true);
        }

        // URL: ~/Project/Details/{ProjectId}
        // Gets the EditForm view.

        [HttpGet]
        public async Task<IActionResult> Edit(int id) // id = Asset Id
        {
            var model = await assetService.GetEditFormAsync(id, User.GetRequiredUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);

        }

        // URL: ~/Project/Details/{ProjectId}
        // Receives the ViewModel and POSTs the new update to Asset.

        [HttpPost]
        public async Task<IActionResult> Edit(EditAssetViewModel model)
        {

            if (!ModelState.IsValid)
            {
                model.Materials = await assetService.GetMaterialsAsync();
                model.Projects = await assetService.GetProjectsAsync();
            }

            bool success = await assetService.EditAsync(model, User.GetRequiredUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Asset has been updated successfully.";
            return RedirectToAction("Details", "Project", new { id = model.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> ReplaceFile(int id)
        {
            var model = await assetService.GetReplaceAssetFileViewModelAsync(id, User.GetRequiredUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // URL: ~/Asset/ReplaceFile/{AssetId}
        // Receives the ViewModel and POSTs the new file to replace the existing Asset file.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplaceFile(int id, ReplaceAssetFileViewModel model) // id = AssetId
        {
            model.AssetId = id;

            if (!ModelState.IsValid)
            {
                var form = await assetService.GetReplaceAssetFileViewModelAsync(id, User.GetRequiredUserId());

                if (form == null)
                {
                    return NotFound();
                }

                model.AssetName = form.AssetName;
                return View(model);
            }

            using Stream? thumbnailStream = model.ThumbnailFile?.OpenReadStream();

            ThumbnailUpload? thumbnail = null;

            if (thumbnailStream != null && model.ThumbnailFile!.Length > 0)
            {
                thumbnail = new ThumbnailUpload
                {
                    Content = thumbnailStream,
                    FileName = model.ThumbnailFile.FileName,
                    Length = model.ThumbnailFile.Length
                };
            }

            bool success = await assetService.ReplaceAssetFileAsync(id, User.GetRequiredUserId(), model, thumbnail);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "File has been replaced and previous revisions were removed.";
            return RedirectToAction("Details", new { id });
        }

        // URL: ~/Product/Details/
        // Deletes an Asset permanently.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) // id = AssetId
        {
            var asset = await assetService.GetDeleteAssetViewModelAsync(id, User.GetRequiredUserId());

            if (asset == null)
            {
                return NotFound();
            }

            bool success = await assetService.DeleteAssetPermanently(id, User.GetRequiredUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Asset file has been deleted successfully.";
            return RedirectToAction("Details", "Project", new { id = asset.ProjectId, updateThumbnail = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRevision(int id)
        {
            var revision = await assetService.GetRevisionFileAsync(id, User.GetRequiredUserId());
            if (revision == null)
            {
                return NotFound();
            }

            string? thumbnailPath = revision.ThumbnailPath;
            string? filePath = await assetService.DeleteRevisionAsync(id, User.GetRequiredUserId());
            if (filePath == null)
            {
                return NotFound();
            }

            thumbnailService.DeleteThumbnail(thumbnailPath);
            fileStorage.DeleteFile(filePath);

            TempData["SuccessMessage"] = "Revision deleted successfully.";
            return RedirectToAction("Details", new { id = revision.AssetId });
        }

        // URL: ~/Asset/Details/Id
        // Adds an updated Revision of Asset to history.

        [HttpGet]
        public async Task<IActionResult> AddRevision(int id)
        {
            var model = await assetService.GetAddRevisionViewModelAsync(id, User.GetRequiredUserId());

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRevision(AddRevisionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var form = await assetService.GetAddRevisionViewModelAsync(model.AssetId, User.GetRequiredUserId());

                if (form == null)
                {
                    return NotFound();
                }

                model.AssetName = form.AssetName;
                model.ProjectId = form.ProjectId;
                model.NextRevisionNumber = form.NextRevisionNumber;

                return View(model);
            }

            using Stream? thumbnailStream = model.ThumbnailFile?.OpenReadStream();

            ThumbnailUpload? thumbnail = null;

            if (thumbnailStream != null && model.ThumbnailFile!.Length > 0)
            {
                thumbnail = new ThumbnailUpload
                {
                    Content = thumbnailStream,
                    FileName = model.ThumbnailFile.FileName,
                    Length = model.ThumbnailFile.Length
                };
            }

            if (!await assetService.CreateRevisionAsync(model, thumbnail, User.GetRequiredUserId()))
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }
    }
}
