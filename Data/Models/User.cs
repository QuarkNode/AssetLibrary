
namespace PartManagementSystem.Data.Models
{
    using Microsoft.AspNetCore.Identity;
    using PartManagementSystem.Data.Models.Enum;

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using static Common.EntityValidation;
    public class User : IdentityUser<int>
    {
        /* This represents a User in the Database */

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
        [Column(TypeName = "DATE")]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        [StringLength(OccupationMaxLength, MinimumLength = OccupationMinLength)]
        public string? Occupation { get; set; }

        [Required]
        [StringLength(CityMaxLength, MinimumLength = CityMinLength)]
        public string City { get; set; } = null!;

        [Required]
        [StringLength(CountryMaxLength, MinimumLength = CountryMinLength)]
        public string Country { get; set; } = null!;

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "DATETIME2(3)")]
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
