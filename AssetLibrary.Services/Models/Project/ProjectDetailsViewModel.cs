
namespace AssetLibrary.Services.Models.Project
{
    using AssetLibrary.Data.Models.Enum;
    using AssetLibrary.Services.Models.Asset;
    public class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public int OwnerId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerFirstName { get; set; } = string.Empty;
        public string OwnerLastName { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        public Status Status { get; set; }
        public List<AssetCardViewModel> Assets { get; set; } = new();
    }
}