
namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    public class ProjectTags
    {
        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
