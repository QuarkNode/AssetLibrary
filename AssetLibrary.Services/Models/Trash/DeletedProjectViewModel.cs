namespace AssetLibrary.Services.Models.Trash
{
    public class DeletedProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public int AssetCount { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
