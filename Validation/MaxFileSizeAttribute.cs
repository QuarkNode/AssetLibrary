
namespace PartManagementSystem.Validation
{
    using System.ComponentModel.DataAnnotations;
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long maxBytes;
        public MaxFileSizeAttribute(int maxMb)
        {
            this.maxBytes = maxMb * 1028L * 1028L;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > maxBytes)
            {
                return new ValidationResult($"File must be smaller than {maxBytes / 1024 / 1024} MB");
            }

            return ValidationResult.Success;
        }
    }
}
