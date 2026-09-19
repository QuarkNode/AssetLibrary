
namespace PartManagementSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static Common.ApplicationConstants;
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [StringLength(TagNameMaxLength, MinimumLength = TagNameMinLength)]
        public string TagName { get; set; } = null!;
        public bool IsCustomTag { get; set; }
    }
}
