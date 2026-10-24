namespace AssetLibrary.Services.Models.Home
{
    using AssetLibrary.Services.Models.Project;
    public class HomeViewModel
    {
        public bool IsLoggedIn { get; set; }

        public List<ProjectCardViewModel> Projects { get; set; } = new();
        public int ProjectCount { get; set; }
    }
}
