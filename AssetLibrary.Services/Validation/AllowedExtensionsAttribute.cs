namespace AssetLibrary.Services.Validation
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] extensions;

        public AllowedExtensionsAttribute(params string[] extension)
        {
            this.extensions = extension;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extensions = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!this.extensions.Contains(extensions))
                {
                    return new ValidationResult($"Only these formats are supported: {string.Join(", ", extensions)}");
                }
            }
            return ValidationResult.Success;
        }
    }
}
