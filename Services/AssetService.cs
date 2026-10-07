
namespace PartManagementSystem.Services
{
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels;
    public class AssetService : IAssetService
    {
        public Task<IEnumerable<AssetCardViewModel>> GetAllAssetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AssetDetailsViewModel> GetAssetDetailsByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
