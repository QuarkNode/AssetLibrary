namespace AssetLibrary.Services.Models.Revision
{
    public class RevisionFileViewModel
    {
        public int RevisionId { get; set; }
        public int AssetId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFIleName { get; set; } = string.Empty;
    }
}
