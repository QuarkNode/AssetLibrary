
namespace AssetLibrary.Services.Models.Asset
{
    using AssetLibrary.Data.Models.Enum;
    public class AssetCardViewModel
    {
        public int AssetId { get; set; }
        public string? PartNumber { get; set; }
        public string AssetName { get; set; } = null!;
        public Status Status { get; set; }
        public string? MaterialName { get; set; }
        public int? CurrentRevisionNumber { get; set; }
        public bool IsDeleted { get; set; }
        public int? RevisionCount { get; set; }
    }
}
