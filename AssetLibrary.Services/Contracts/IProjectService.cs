using AssetLibrary.Services.Models;
using AssetLibrary.Services.Models.Asset;
using AssetLibrary.Services.Models.Project;

namespace AssetLibrary.Services.Contracts
{
    public interface IProjectService
    {
        Task<ProjectDetailsViewModel?> GetDetailsAsync(int projectId);

        Task<UploadAssetViewModel> GetAssetUploadViewModelAsync();

        Task<AddAssetViewModel> AddNewAssetUploadViewModelAsync(int routeId, string projectName);

        Task ProcessAssetUploadViewModelAsync(AddAssetViewModel model, ThumbnailUpload? thumbnail, int userId);

        Task<int> ProcessAssetUploadViewModelAsync(UploadAssetViewModel viewModel, ThumbnailUpload? thumbnail, int userId);

        Task<string?> GetProjectNameAsync(int routeProjectId, int userId);

        Task<bool> DeleteProjectAsync(int projectId, int userId);
        Task<EditProjectViewModel?> GetEditFormAsync(int projectId, int userId);
        Task<bool> EditAsync(EditProjectViewModel model, int userId);
        Task<List<string>?> GetProjectModelPathsAsync(int projectId, int userId);
    }
}
