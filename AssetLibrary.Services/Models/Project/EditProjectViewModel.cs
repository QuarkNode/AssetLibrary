namespace AssetLibrary.Services.Models.Project
{
    using AssetLibrary.Data.Models.Enum;

    using System.ComponentModel.DataAnnotations;
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }

        [Display(Name = "Project Name")]
        [Required(ErrorMessage = "Project name is required!")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Project name must be between {2} and {1} symbols.")]
        public string ProjectName { get; set; } = null!;

        [MaxLength(150, ErrorMessage = "Too long description.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose a status")]
        [EnumDataType(typeof(Status), ErrorMessage = "Invalid status")]
        public Status Status { get; set; }
    }
}
