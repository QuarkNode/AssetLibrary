using AssetLibrary.Data.Models.Enum;

namespace AssetLibrary.Services.Models.Trash
{
    using AssetLibrary.Services.Models.Asset;
    public class DeletedProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public string OwnerFirstName { get; set; } = null!;
        public string OwnerLastName { get; set; } = null!;
        public Status Status { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<AssetCardViewModel> Assets { get; set; } = new();
    }
}
