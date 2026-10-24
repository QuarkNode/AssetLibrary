using AssetLibrary.Services.Models.Trash;

namespace AssetLibrary.Services.Contracts
{
    public interface ITrashService
    {
        Task<List<DeletedProjectViewModel>> GetDeletedProjectsViewModelAsync(int userId);
        Task<DeletedProjectDetailsViewModel?> GetDeletedProjectDetailsAsync(int projectId);
        Task<bool> RestoreProjectAsync(int projectId, int userId);
        Task<bool> DeletePermanentlyAsync(int projectId, int userId);
    }
}
