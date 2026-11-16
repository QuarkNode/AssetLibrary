namespace AssetLibrary.Services.Models.Asset
{
    public class DeleteAssetViewModel
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
    }
}
