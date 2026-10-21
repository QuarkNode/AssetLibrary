
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await trashService.GetDeletedProjectsViewModelAsync(User.GetRequiredUserId());

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project = await trashService.GetDeletedProjectDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public async Task<IActionResult> Restore() // To-Do
        {

        }

        public async Task<IActionResult> DeletePermanently() // To-Do
        {

        }
    }
}
