
namespace PartManagementSystem.Services
{
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels.Asset;
    using PartManagementSystem.ViewModels.Revision;

    public class AssetService : IAssetService
    {
        public Task<bool> AddRevisionAsync(AddRevisionViewModel model, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<int?> CreateAsync(AddAssetViewModel model, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int assetId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditAsync(EditAssetViewModel model, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<AddRevisionViewModel?> GetAddRevisionFormAsync(int assetId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetDetailsViewModel>> GetDetailsAsync(int assetId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<EditAssetViewModel?> GetEditFormAsync(int assetId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<RevisionFileViewModel?> GetRevisionFileAsync(int revisionId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
