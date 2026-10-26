using AssetLibrary.Extensions;
using AssetLibrary.Services.Contracts;
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
        public AssetController(IAssetService assetService, IFileStorageService fileStorage)
        {
            this.assetService = assetService;
            this.fileStorage = fileStorage;
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

            bool success = await assetService.ReplaceAssetFileAsync(id, User.GetRequiredUserId(), model);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "File replaced. Previous revisions were removed.";
            return RedirectToAction("Details", new { id });
        }

        // URL: ~/Product/Details/
        // Deletes an Asset permanently.

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await assetService.GetAssetDetailsAsync(id, User.GetRequiredUserId());

            if (asset == null)
            {
                return NotFound();
            }

            bool success = await assetService.DeleteAssetPermanently(asset);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Asset file deleted successfully.";
            return RedirectToAction("Details", "Project", new { id = asset.ProjectId });
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

            string? filePath = await assetService.DeleteRevisionAsync(id, User.GetRequiredUserId());
            if (filePath == null)
            {
                return NotFound();
            }

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

            if (!await assetService.CreateRevisionAsync(model, User.GetRequiredUserId()))
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }
    }
}
