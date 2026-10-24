namespace AssetLibrary.Controllers
{
    using AssetLibrary.Services.Contracts;
    using Microsoft.AspNetCore.Mvc;

    using System.Security.Claims;

    public class HomeController : Controller
    {
        private readonly IHomeService homeService;
        public HomeController(IHomeService homeService)
        {
            this.homeService = homeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId = GetUserId();
            var viewModel = await homeService.GetHomeViewModelAsync(userId);

            return View(viewModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        // Method
        private int? GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : null;
        }
    }

}
