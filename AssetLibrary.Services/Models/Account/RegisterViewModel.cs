namespace AssetLibrary.Services.Models.Account
{
    using AssetLibrary.Data.Models.Enum;
    using AssetLibrary.Services.Validation;
    using System.ComponentModel.DataAnnotations;
    using static AssetLibrary.Data.Common.ViewModelValidation;

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(RegisterUsernameMaxLength, MinimumLength = RegisterUsernameMinLength,
            ErrorMessage = "Username needs to be between {2} and {1} symbols")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your email")]
        [Compare(nameof(Email), ErrorMessage = "Email does not match")]
        [Display(Name = "Confirm Email")]
        public string ConfirmEmail { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(RegisterFirstNameMaxLength, MinimumLength = RegisterFirstNameMinLength,
            ErrorMessage = "First name needs to be between {2} and {1} symbols")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(RegisterLastNameMaxLength, MinimumLength = RegisterLastNameMinLength,
            ErrorMessage = "Last name needs to be between {2} and {1} symbols")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.DateTime)]
        [MinimumAge(RegisterMinimumRegistrationAge)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "City is required")]
        [StringLength(RegisterCityMaxLength, MinimumLength = RegisterCityMinLength)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(RegisterCountryMaxLength, MinimumLength = RegisterCountryMinLength)]
        public string Country { get; set; } = null!;
    }
}
