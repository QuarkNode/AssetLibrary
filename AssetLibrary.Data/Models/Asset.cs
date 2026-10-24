namespace AssetLibrary.Data.Models
{
    using AssetLibrary.Data.Models.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static Common.EntityValidation;
    public class Asset
    {
        /* This represents one 3D part/file conceptually. Not any single version of it - look at Revision for this. It holds the identity and current state of the part. */

        [Key]
        public int AssetId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;

        [MaxLength(PartNumberMaxLength)]
        public string? PartNumber { get; set; }

        [Required]
        [StringLength(AssetNameMaxLength, MinimumLength = AssetNameMinLength)]
        public string AssetName { get; set; } = null!;

        [StringLength(AssetDescriptionMaxLength)]
        public string? Description { get; set; }
        public int? MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public Material? Material { get; set; }

        [Required]
        public Status Status { get; set; }
        public int? CurrentRevisionId { get; set; }

        [ForeignKey(nameof(CurrentRevisionId))]
        public Revision? CurrentRevision { get; set; }

        public int OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public User Owner { get; set; } = null!;

        [Required]
        [Column(TypeName = "DATETIME2(3)")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime? UpdatedOn { get; set; }

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Revision> Revisions { get; set; } = new List<Revision>();
    }
}
