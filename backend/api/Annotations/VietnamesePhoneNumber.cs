using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace api.Annotations
{
    public class VietnamesePhoneNumber : ValidationAttribute
    {
        private static readonly List<string> ValidPrefixes = new List<string>
        {
            "013", "016", "032", "033", "034", "035", "036", "037",
            "038", "039", "052", "055", "056", "058", "059", "070",
            "076", "077", "078", "079", "081", "082", "083", "084",
            "085", "086", "088", "089", "090", "091", "092", "093",
            "094", "096", "097", "098", "099"
        };
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            string phoneNumber = value.ToString()!;
            string pattern = @"^(0[\d]{9})$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (!IsValid(value))
                return new ValidationResult(ErrorMessage ?? "Invalid Vietnamese phone number format");

            if (value == null)
                return ValidationResult.Success!;

            string phoneNumber = value!.ToString()!;
            string prefix = phoneNumber.Substring(0, 3);

            if (!ValidPrefixes.Contains(prefix))
                return new ValidationResult(ErrorMessage ?? "Invalid Vietnamese phone number prefix");

            return ValidationResult.Success!;
        }
    }
}