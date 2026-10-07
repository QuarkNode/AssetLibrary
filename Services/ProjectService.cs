
namespace PartManagementSystem.Services
{
    using PartManagementSystem.Services.Contracts;
    using PartManagementSystem.ViewModels.Project;

    public class ProjectService : IProjectService
    {
        public Task<ProjectDetailsViewModel> GetDetailsAsync(int projectId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNameAsync(int projectId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProjectCardViewModel>> GetUserProjectsAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
