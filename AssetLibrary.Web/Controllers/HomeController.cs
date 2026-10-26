namespace AssetLibrary.Controllers
{
    using AssetLibrary.Extensions;
    using AssetLibrary.Services.Contracts;

    using Microsoft.AspNetCore.Mvc;

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
            var viewModel = await homeService.GetHomeViewModelAsync(User.GetRequiredUserId());

            return View(viewModel);
        }
    }

}
