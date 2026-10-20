namespace AssetLibrary.Validation
{
    using System.ComponentModel.DataAnnotations;
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int minAge;
        public MinimumAgeAttribute(int minAge)
        {
            this.minAge = minAge;

        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime birthDate)
            {
                DateTime latestAllowed = DateTime.Today.AddYears(-this.minAge);

                if (birthDate.Date > latestAllowed)
                {
                    return new ValidationResult(ErrorMessage = $"You must be at least {this.minAge} years old.");
                }

                if (birthDate.Year < 1940)
                {
                    return new ValidationResult(ErrorMessage = $"Please enter a valid birth date.");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date format");
        }
    }
}
