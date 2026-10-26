namespace AssetLibrary.Services.Validation
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] allowedExtensions;

        public AllowedExtensionsAttribute(params string[] extensions)
        {
            this.allowedExtensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                string uploadedExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(uploadedExtension))
                {
                    return new ValidationResult(
                        $"Only these formats are supported: {string.Join(", ", allowedExtensions)}");
                }
            }

            return ValidationResult.Success;
        }
    }
}