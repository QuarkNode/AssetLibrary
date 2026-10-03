
namespace PartManagementSystem.Data.Models
{
    using PartManagementSystem.Data.Models.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static Common.EntityValidation;

    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(ProjectNameMaxLength, MinimumLength = ProjectNameMinLength)]
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
        public bool IsDeleted { get; set; }

        [Required]
        public Status Status { get; set; }

        [StringLength(ProjectDescriptionMaxLength)]
        public string? Description { get; set; }

        public ICollection<Asset> Assets = new List<Asset>();
    }
}