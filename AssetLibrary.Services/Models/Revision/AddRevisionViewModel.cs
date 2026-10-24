namespace AssetLibrary.Services.Models.Revision
{
    using AssetLibrary.Services.Validation;
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;
    using static AssetLibrary.Data.Common.ViewModelValidation;
    public class AddRevisionViewModel
    {
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public int ProjectId { get; set; }
        public int NextRevisionNumber { get; set; }

        [Required(ErrorMessage = "Please pick a file")]
        [AllowedExtensions(".obj", ".stl", ".fbx")]
        [MaxFileSize(75)]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "Please describe what changed")]
        [MaxLength(ChangeNotesMaxLength)]
        public string ChangeNotes { get; set; } = null!;
    }
}
