using AssetLibrary.Services.Contracts;
using AssetLibrary.Services.Models.Asset;
using AssetLibrary.Services.Models.Revision;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

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
        public async Task<IActionResult> Details(int id)
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

            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var revision = await assetService.GetRevisionFileAsync(id, userId.Value);

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
        [HttpGet]
        public async Task<IActionResult> Edit(int id) // id = Asset Id
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Challenge();
            }

            var model = await assetService.GetEditFormAsync(id, userId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAssetViewModel model)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model.Materials = await assetService.GetMaterialsAsync();
                model.Projects = await assetService.GetProjectsAsync();
            }

            bool sucess = await assetService.EditAsync(model, userId.Value);

            if (!sucess)
            {
                return NotFound();
            }

            return RedirectToAction("Details", "Project", new { id = model.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> ReplaceFile(int id)
        {
            int? userId = GetUserId(); // Assuming you have a helper method for this

            if (!userId.HasValue)
            {
                return Challenge();
            }

            var model = await assetService.GetReplaceAssetFileViewModelAsync(id, userId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplaceFile(ReplaceAssetFileViewModel model)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool success = await assetService.ReplaceCurrentRevisionFileAsync(model.AssetId, userId.Value, model);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Asset file replaced successfully.";
            return RedirectToAction("Details", new { id = model.AssetId });
        }

        // URL: ~/Asset/Details/Id
        // Adds an updated Revision of Asset to hisotry.

        [HttpGet]
        public async Task<IActionResult> AddRevision(int id)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var model = await assetService.GetAddRevisionViewModelAsync(id, userId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRevision(AddRevisionViewModel model)
        {
            int? userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            // Populate metadata and ensure asset exists/belongs to user
            bool exists = await assetService.PopulateRevisionMetadataAsync(model, userId.Value);
            if (!exists)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool success = await assetService.CreateRevisionAsync(model, userId.Value);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        private int? GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : null;
        }


    }
}
