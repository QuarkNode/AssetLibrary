namespace AssetLibrary.Services.Models
{
    public class ThumbnailUpload
    {
        public Stream Content { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public long Length { get; set; }
    }
}
