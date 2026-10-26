namespace AssetLibrary.Validation
{
    using System.ComponentModel.DataAnnotations;
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int maxMb;
        private readonly long maxBytes;
        public MaxFileSizeAttribute(int maxMb)
        {
            this.maxMb = maxMb;
            this.maxBytes = maxMb * 1024L * 1024L;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > maxBytes)
            {
                return new ValidationResult($"File must be smaller than {maxMb} MB");
            }

            return ValidationResult.Success;
        }
    }
}
