namespace PartManagementSystem.ViewModels.Home
{
    using PartManagementSystem.ViewModels.Project;
    using System.ComponentModel.DataAnnotations;
    using static Common.ViewModelValidation;
    public class HomeViewModel
    {
        [Required(ErrorMessage = "You must be logged in!")]
        public bool IsLoggedIn { get; set; }

        public List<ProjectCardViewModel> Projects { get; set; } = new();

        [Range(0, ProjectCountMaxLength, ErrorMessage = "Exceeded Project amount!")]
        public int ProjectCount { get; set; }
    }
}
