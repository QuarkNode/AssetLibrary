
namespace PartManagementSystem.Controllers
{
    using Microsoft.EntityFrameworkCore;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using PartManagementSystem.Data;
    using PartManagementSystem.ViewModels;

    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public ProjectController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;

        }

        [HttpGet]
        public async Task<IActionResult> Index() // This returns a list of available Projects and Materials from DB.
        {
            var viewModel = new AssetUploadViewModel
            {
                Projects = await dbContext.Projects
                .Select(p => new SelectListItem { Value = p.ProjectId.ToString(), Text = p.ProjectName})
                .ToListAsync(),

                Materials = await dbContext.Materials
                .Select(m => new SelectListItem { Value = m.MaterialId.ToString(), Text = m.MaterialName})
                .ToListAsync()
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {

            var asset = await dbContext.Assets
                .Include(a => a.Project)
                .Include(a => a.Material)
                .Include(a => a.Owner)
                .Include(a => a.CurrentRevision)
                .Include(a => a.Revisions)
                .FirstOrDefaultAsync(a => a.AssetId == id && !a.IsDeleted);

            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }
    }
}
