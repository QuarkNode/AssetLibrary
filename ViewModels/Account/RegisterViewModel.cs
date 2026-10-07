namespace PartManagementSystem.ViewModels.Account
{

    using PartManagementSystem.Data.Models.Enum;
    using PartManagementSystem.Validation;
    using System.ComponentModel.DataAnnotations;
    using static Common.ViewModelValidation;
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(UsernameMaxLength, MinimumLength = UsernameMinLength,
            ErrorMessage = "Username needs to be between {2} and {1} symbols")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(FirstNameMaxLength, MinimumLength = FirstNameMinLength,
            ErrorMessage = "First name needs to be between {2} and {1} symbols")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(LastNameMaxLength, MinimumLength = LastNameMinLength,
            ErrorMessage = "Last name needs to be between {2} and {1} symbols")]
        public string LastName { get; set; } = null!;

        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.DateTime)]
        [MinimumAge(MinimumRegistrationAge)]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "City is required")]
        [StringLength(CityMaxLength, MinimumLength = CityMinLength)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(CountryMaxLength, MinimumLength = CountryMinLength)]
        public string Country { get; set; } = null!;
    }
}
