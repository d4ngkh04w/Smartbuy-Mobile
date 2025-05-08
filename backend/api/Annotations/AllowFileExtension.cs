using System.ComponentModel.DataAnnotations;

namespace api.Annotations
{
    public class AllowFileExtension : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowFileExtension(string[] extensions)
        {
            _extensions = extensions;
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