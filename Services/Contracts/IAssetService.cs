
namespace PartManagementSystem.Services.Contracts
{
    using PartManagementSystem.ViewModels.Asset;
    using PartManagementSystem.ViewModels.Revision;

    public interface IAssetService
    {
        /* Description: Asset details, create, add revision, edit, soft delete; Never decides what HTTP result to return. */

        // Read
        Task<IEnumerable<AssetDetailsViewModel>> GetDetailsAsync(int assetId, int userId);

        // Create: returns the new asset's id, or null if the project isn't the user's
        Task<int?> CreateAsync(AddAssetViewModel model, int userId);

        // Revisions
        Task<AddRevisionViewModel?> GetAddRevisionFormAsync(int assetId, int userId);
        Task<bool> AddRevisionAsync(AddRevisionViewModel model, int userId);

        // Update
        Task<EditAssetViewModel?> GetEditFormAsync(int assetId, int userId);
        Task<bool> EditAsync(EditAssetViewModel model, int userId);

        // Delete (soft): true if it was deleted, false if not found or not owned
        Task<bool> DeleteAsync(int assetId, int userId);

        // For downloads: file path and original name of a revision; null if not found or not owned

        Task<RevisionFileViewModel?> GetRevisionFileAsync(int revisionId, int userId);

    }
}
