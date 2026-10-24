namespace AssetLibrary.Services.Models.Asset
{
    using AssetLibrary.Services.Validation;
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;
    public class ReplaceAssetFileViewModel
    {
        [Required]
        public int AssetId { get; set; }

        [Display(Name = "Asset Name")]
        public string? AssetName { get; set; }

        [Required(ErrorMessage = "Please select a replacement file.")]
        [AllowedExtensions(".obj", ".stl", ".fbx")]
        [MaxFileSize(75)]
        public IFormFile File { get; set; } = null!;


    }
}
