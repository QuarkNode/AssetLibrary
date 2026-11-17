using AssetLibrary.Services.Models;

namespace AssetLibrary.Services.Contracts
{
    public interface IThumbnailService
    {
        Task<byte[]> GenerateAssetThumbnailAsync(string rendererUrl, string modelUrl);
        Task<byte[]> GenerateProjectThumbnailAsync(string rendererUrl, IReadOnlyCollection<string> modelUrls);

        Task<string> SaveAssetThumbnailAsync(int revisionId, ThumbnailUpload thumbnail);
        Task<string> SaveProjectThumbnailAsync(int projectId, ThumbnailUpload thumbnail);
        Task ClearProjectThumbnailAsync(int projectId);

        void DeleteThumbnail(string? relativePath);
    }
}