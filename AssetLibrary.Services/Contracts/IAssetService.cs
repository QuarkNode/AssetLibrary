namespace AssetLibrary.Services.Contracts
{
    using AssetLibrary.Data.Models;
    using AssetLibrary.Services.Models;
    using AssetLibrary.Services.Models.Asset;
    using AssetLibrary.Services.Models.Revision;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public interface IAssetService
    {
        public Task<Asset?> GetAssetDetailsAsync(int assetId);
        public Task<Asset?> GetAssetDetailsAsync(int assetId, int userId);
        public Task<DeleteAssetViewModel?> GetDeleteAssetViewModelAsync(int assetId, int userId);
        public Task<RevisionFileViewModel?> GetRevisionFileAsync(int revisionId, int userId);
        public Task<RevisionFileViewModel?> GetCurrentRevisionFileAsync(int assetId, int userId);
        public Task<AddRevisionViewModel?> GetAddRevisionViewModelAsync(int assetId, int userId);
        public Task<bool> CreateRevisionAsync(AddRevisionViewModel model, ThumbnailUpload? thumbnail, int userId);
        public Task<EditAssetViewModel?> GetEditFormAsync(int assetId, int userId);
        public Task<bool> EditAsync(EditAssetViewModel model, int userId);
        public Task<IEnumerable<SelectListItem>> GetMaterialsAsync();
        public Task<IEnumerable<SelectListItem>> GetProjectsAsync();
        public Task<ReplaceAssetFileViewModel?> GetReplaceAssetFileViewModelAsync(int assetId, int userId);
        public Task<bool> ReplaceAssetFileAsync(int assetId, int userId, ReplaceAssetFileViewModel model, ThumbnailUpload? thumbnail);
        public Task<bool> DeleteAssetPermanently(int id, int userId);
        public Task<string?> DeleteRevisionAsync(int revisionId, int userId);
    }
}
