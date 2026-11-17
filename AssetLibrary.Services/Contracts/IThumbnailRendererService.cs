namespace AssetLibrary.Services.Contracts
{
    public interface IThumbnailRendererService
    {
        public Task<byte[]> RenderAssetAsync(string rendererUrl, string modelUrl);
        public Task<byte[]> RenderProjectAsync(string rendererUrl, IReadOnlyCollection<string> modelUrls);
    }
}
