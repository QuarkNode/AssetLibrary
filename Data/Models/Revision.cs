namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static Common.ApplicationConstants;
    public class Revision
    {
        [Key]
        public int RevisionId { get; set; }
        public int AssetId { get; set; }

        [ForeignKey(nameof(AssetId))]
        public Asset Asset { get; set; } = null!;

        [Required]
        [StringLength(FilePathMaxLength)]
        public string FilePath { get; set; } = null!;

        [Required]
        [StringLength(FileExtensionMaxLength, MinimumLength = FileExtensionMinLength)]
        public string FileExtension { get; set; } = null!;

        [Required]
        public long FileSizeBytes { get; set; }

        [StringLength(RevisionChangeNotesMaxLength)]
        public string? ChangeNotes { get; set; }
        public int RevisionNumber { get; set; }

        [Required]
        public int UploadedByUserId { get; set; }

        [ForeignKey(nameof(UploadedByUserId))]
        public User UploadedByUser { get; set; } = null!;

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime UploadedOn { get; set; }
    }
}