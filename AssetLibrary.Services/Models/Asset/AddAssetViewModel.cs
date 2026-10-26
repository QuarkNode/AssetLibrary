namespace AssetLibrary.Services.Models.Asset
{
    using AssetLibrary.Services.Validation;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using System.ComponentModel.DataAnnotations;

    using static AssetLibrary.Data.Common.CommonValidation;
    using static AssetLibrary.Data.Common.ViewModelValidation;
    public class AddAssetViewModel
    {
        public int ProjectId { get; set; }

        [Display(Name = "Project Name")]
        public string? ProjectName { get; set; }

        [Display(Name = "Asset Name")]
        [Required(ErrorMessage = "Asset Name is required")]
        [StringLength(AssetNameMaxLength, MinimumLength = AssetNameMinLength,
            ErrorMessage = "Asset Name needs to be between {2} and {1} symbols")]
        public string AssetName { get; set; } = null!;

        [Display(Name = "Part Number")]
        [StringLength(AssetPartNumberMaxLength, MinimumLength = AssetPartNumberMinLength,
            ErrorMessage = "Part Number needs to be between {2} and {1} symbols")]
        public string? PartNumber { get; set; }

        [MaxLength(AssetDescriptionMaxLength, ErrorMessage = "Description is too long")]
        public string? Description { get; set; }

        [Display(Name = "Material")]
        public int? MaterialId { get; set; }

        [Required(ErrorMessage = "Please pick a file")]
        [AllowedExtensions(".obj", ".stl", ".fbx")]
        [MaxFileSize(MaxFileSizeInMB)]
        public IFormFile File { get; set; } = null!;
        public IEnumerable<SelectListItem>? Materials { get; set; }

    }
}
