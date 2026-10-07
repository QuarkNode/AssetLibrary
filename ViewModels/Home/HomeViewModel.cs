namespace PartManagementSystem.ViewModels.Home
{
    using PartManagementSystem.ViewModels.Project;
    public class HomeViewModel
    {
        public bool IsLoggedIn { get; set; }

        public List<ProjectCardViewModel> Projects { get; set; } = new();
        public int ProjectCount { get; set; }
    }
}
