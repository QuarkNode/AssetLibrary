using PartManagementSystem.ViewModels.Project;

namespace PartManagementSystem.Services.Contracts
{
    /* Description: Project queries (user's list, details, dropdown options), creating projects; Never touches files or HttpContext. */

    public interface IProjectService
    {
        // Home page cards for the logged-in user
        Task<IEnumerable<ProjectCardViewModel>> GetUserProjectsAsync(int userId);

        // null = project doesn't exist or isn't owned by this user
        Task<ProjectDetailsViewModel> GetDetailsAsync(int projectId, int userId);

        // Project name for the "Add asset" form header; null = not found or not owned
        Task<string?> GetNameAsync(int projectId, int userId);
    }
}
