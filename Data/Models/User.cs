
namespace PartManagementSystem.Data.Models
{
    using PartManagementSystem.Data.Models.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static Common.ApplicationConstants;
    public class User
    {
        /* This represents a User in the Database */
        [Key]
        public int UserId { get; set; }

        [StringLength(CompanyNameMaxLength, MinimumLength = CompanyNameMinLength)]
        public string? CompanyName { get; set; }

        [Required]
        [StringLength(FirstNameMaxLength, MinimumLength = FirstNameMinLength)]
        public string FirstName { get; set; } = null!;

        [StringLength(MiddleNameMaxLength, MinimumLength = MiddleNameMinLength)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(LastNameMaxLength, MinimumLength = LastNameMinLength)]
        public string LastName { get; set; } = null!;

        [Required]
        [Range(AgeMinLength, AgeMaxLength)]
        public int Age { get; set; }
        public Gender Gender { get; set; }

        [Required]
        [StringLength(EmailMaxLength, MinimumLength = EmailMinLength)]
        public string Email { get; set; } = null!;

        public string? Occupation { get; set; }

        [Required]
        [StringLength(CityMaxLength, MinimumLength = CityMinLength)]
        public string City { get; set; } = null!;

        [Required]
        [StringLength(CountryMaxLength, MinimumLength = CountryMinLength)]
        public string Country { get; set; } = null!;

        [Required]
        [Column(TypeName = "DATETIME2(3)")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
