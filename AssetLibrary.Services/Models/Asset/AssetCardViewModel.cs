using AssetLibrary.Data.Models.Enum;

namespace AssetLibrary.Services.Models.Asset
{
    public class AssetCardViewModel
    {
        public int AssetId { get; set; }
        public string PartNumber { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public Status Status { get; set; }
        public string MaterialName { get; set; } = null!;
        public string CurrentRevisionNumber { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public int? RevisionCount { get; set; }
    }
}
