namespace AssetLibrary.Controllers
{
    using AssetLibrary.Extensions;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Home;
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        private readonly IHomeService homeService;

        public HomeController(IHomeService homeService)
        {
            this.homeService = homeService;
        }

        // URL: ~/Home/Index
        // Returns a view of the Home page with a list of Projects for the logged-in user.

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await homeService.GetHomeViewModelAsync(User.GetUserId());

            return View(viewModel);
        }

        public IActionResult Viewer()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error([FromQuery] int statusCode)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, StatusCode = statusCode });
        }
    }

}
