namespace PartManagementSystem.ViewModels.Project
{
    public class ProjectCardViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public int AssetCount { get; set; }
    }
}