using System.ComponentModel.DataAnnotations;

namespace api.Extensions
{
    public class AllowFileExtension : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowFileExtension(string[] Extensions)
        {
            _extensions = Extensions;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success!;

        }
    }
}