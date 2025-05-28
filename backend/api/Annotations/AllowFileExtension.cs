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
                    return new ValidationResult(ErrorMessage ?? $"File extension must be one of: {string.Join(", ", _extensions)}");
                }
            }
            else if (value is IEnumerable<IFormFile> files)
            {
                foreach (var f in files)
                {
                    var extension = Path.GetExtension(f.FileName)?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension))
                    {
                        return new ValidationResult(ErrorMessage ?? $"File extension must be one of: {string.Join(", ", _extensions)}");
                    }
                }
            }
            return ValidationResult.Success!;
        }
    }
}