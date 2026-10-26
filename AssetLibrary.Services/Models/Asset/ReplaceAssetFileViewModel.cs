namespace AssetLibrary.Services.Models.Asset
{
    using AssetLibrary.Services.Validation;

    using Microsoft.AspNetCore.Http;

    using System.ComponentModel.DataAnnotations;

    using static AssetLibrary.Data.Common.CommonValidation;
    public class ReplaceAssetFileViewModel
    {
        [Required]
        public int AssetId { get; set; }

        [Display(Name = "Asset Name")]
        public string? AssetName { get; set; }

        [Required(ErrorMessage = "Please select a replacement file.")]
        [AllowedExtensions(".obj", ".stl", ".fbx")]
        [MaxFileSize(MaxFileSizeInMB)]
        public IFormFile File { get; set; } = null!;


    }
}
