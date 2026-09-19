
namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using PartManagementSystem.Data.Models.Enum;

    using static Common.ApplicationConstants;

    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        public string ProjectName { get; set; } = null!;

        [Required]
        public int OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public User Owner { get; set; } = null!;

        [Required]
        [Column(TypeName = "DATETIME2(3)")]
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        [Required]
        public Status Status { get; set; }

        [StringLength(ProjectDescriptionMaxLength)]
        public string? Description { get; set; }

    }
}