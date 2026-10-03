
namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Common.EntityValidation;
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required]
        [StringLength(MaterialNameMaxLength, MinimumLength = MaterialNameMinLength)]
        public string MaterialName { get; set; } = null!;

    }
}