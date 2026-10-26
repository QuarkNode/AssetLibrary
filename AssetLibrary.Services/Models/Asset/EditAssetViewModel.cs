namespace AssetLibrary.Services.Models.Asset
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.ComponentModel.DataAnnotations;
    public class EditAssetViewModel
    {
        public int AssetId { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [Display(Name = "Project Name")]
        [Required(ErrorMessage = "Project name is required!")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Project name must be between {2} and {1} symbols.")]
        public string AssetName { get; set; } = null!;

        [MaxLength(150, ErrorMessage = "Too long description.")]
        public string? Description { get; set; }
        public int? MaterialId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public IEnumerable<SelectListItem>? Materials { get; set; }
        public int ProjectId { get; set; }
        public IEnumerable<SelectListItem>? Projects { get; set; }
    }
}
