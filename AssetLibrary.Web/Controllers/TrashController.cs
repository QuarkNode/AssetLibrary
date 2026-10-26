
namespace AssetLibrary.Controllers
{
    using AssetLibrary.Extensions;
    using AssetLibrary.Services.Contracts;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class TrashController : Controller
    {
        private readonly ITrashService trashService;
        public TrashController(ITrashService trashService)
        {
            this.trashService = trashService;
        }

        // URL: ~/Trash/Index
        // Returns a view of the Trash page with a list of deleted Projects for the logged-in user.

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await trashService.GetDeletedProjectsViewModelAsync(User.GetRequiredUserId());

            return View(projects);
        }

        // URL: ~/Trash/Details/id
        // Returns a view of a deleted Project with its information.

        [HttpGet]
        public async Task<IActionResult> Details(int id) // id = ProjectId
        {
            var project = await trashService.GetDeletedProjectDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await trashService.RestoreProjectAsync(id, User.GetRequiredUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Project has been restored!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var success = await trashService.DeletePermanentlyAsync(id, User.GetRequiredUserId());

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Project has been permanently deleted!";
            return RedirectToAction("Index");
        }
    }
}
