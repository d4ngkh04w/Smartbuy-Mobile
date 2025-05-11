using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace api.Annotations
{
    public class Email : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            string email = value.ToString()!;
            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            return Regex.IsMatch(email, pattern);
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (!IsValid(value))
                return new ValidationResult(ErrorMessage ?? "Invalid email format");

            return ValidationResult.Success!;
        }
    }
}