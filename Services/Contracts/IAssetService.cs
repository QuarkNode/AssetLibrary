
namespace PartManagementSystem.Services.Contracts
{
    using PartManagementSystem.ViewModels;
    public interface IAssetService
    {
        /* Description: Asset details, create, add revision, edit, soft delete; Never decides what HTTP result to return. */
        Task<IEnumerable<AssetCardViewModel>> GetAllAssetsAsync();
        Task<AssetDetailsViewModel> GetAssetDetailsByIdAsync(int id);

    }
}
