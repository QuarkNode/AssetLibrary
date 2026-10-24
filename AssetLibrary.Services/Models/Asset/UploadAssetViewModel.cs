namespace AssetLibrary.Services.Models.Asset
{
    using AssetLibrary.Services.Validation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.ComponentModel.DataAnnotations;
    using static AssetLibrary.Data.Common.ViewModelValidation;
    public class UploadAssetViewModel : IValidatableObject
    {
        [Display(Name = "Asset Name")]
        [Required(ErrorMessage = "Asset Name is required")]
        [StringLength(AssetNameMaxLength, MinimumLength = AssetNameMinLength,
            ErrorMessage = "Asset Name needs to be between {2} and {1} symbols")]
        public string AssetName { get; set; } = null!;

        [Display(Name = "Part Number")]
        [StringLength(PartNumberMaxLength, MinimumLength = PartNumberMinLength,
            ErrorMessage = "Part Number needs to be between {2} and {1} symbols")]
        public string? PartNumber { get; set; }

        [MaxLength(AssetUploadDescriptionMaxLength, ErrorMessage = "Description is too long")]
        public string? Description { get; set; }

        [Display(Name = "Project")]
        public int? ProjectId { get; set; }

        [Display(Name = "Material")]
        public int? MaterialId { get; set; }

        [Required(ErrorMessage = "Please pick a file")]
        [AllowedExtensions(".obj", ".stl", ".fbx")]
        [MaxFileSize(75)]
        public IFormFile File { get; set; } = null!;
        public bool IsNewProject { get; set; }

        [Display(Name = "New Project Name")]
        [StringLength(NewProjectNameMaxLenght, MinimumLength = NewProjectNameMinLenght, ErrorMessage = "New Project name needs to be between {2} and {1} symbols")]
        public string? NewProjectName { get; set; }

        [Display(Name = "New Project Description")]
        [MaxLength(NewProjectDescriptionMaxlength, ErrorMessage = "Description is too long")]
        public string? NewProjectDescription { get; set; }
        public IEnumerable<SelectListItem>? Projects { get; set; }
        public IEnumerable<SelectListItem>? Materials { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsNewProject && string.IsNullOrWhiteSpace(NewProjectName))
            {
                yield return new ValidationResult("Project name is required.", new[] { nameof(NewProjectName) });
            }

            if (!IsNewProject && ProjectId == null)
            {
                yield return new ValidationResult("Please select a project.", new[] { nameof(ProjectId) });
            }
        }
    }
}
