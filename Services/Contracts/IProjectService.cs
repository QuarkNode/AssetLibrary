using PartManagementSystem.Data.Models;
using PartManagementSystem.ViewModels.Asset;

namespace PartManagementSystem.Services.Contracts
{
    /* Description: Project queries (user's list, details, dropdown options), creating projects; Never touches files or HttpContext. */

    public interface IProjectService
    {
        // Methods the controller currently uses
        Task<Project?> GetDetailsAsync(int projectId);

        Task<AssetUploadViewModel> GetAssetUploadViewModelAsync();

        Task<AddAssetViewModel> AddNewAssetUploadViewModelAsync(int routeId, string projectName);

        Task ProcessAssetUploadViewModelAsync(AddAssetViewModel model, int userId);

        Task<int> ProcessAssetUploadViewModelAsync(AssetUploadViewModel viewModel, int userId);

        Task<string?> GetProjectNameAsync(int routeProjectId, int userId);
    }
}
