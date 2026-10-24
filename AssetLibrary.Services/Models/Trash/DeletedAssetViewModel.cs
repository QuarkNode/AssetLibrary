using AssetLibrary.Data.Models.Enum;

namespace AssetLibrary.Services.Models.Trash
{
    public class DeletedAssetViewModel
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = null!;
        public Status Status { get; set; }
        public int RevisionCount { get; set; }
    }
}
