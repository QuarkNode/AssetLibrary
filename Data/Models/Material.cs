
namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static Common.ApplicationConstants;
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required]
        [StringLength(MaterialNameMaxLength, MinimumLength = MaterialNameMinLength)]
        public string MaterialName { get; set; } = null!;

        [Column(TypeName = "DECIMAL(8,2)")]
        public decimal? Density { get; set; }

    }
}