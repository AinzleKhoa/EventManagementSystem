using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AuthenticationAPI.Validations
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Allow null or empty (Use [Required] in DTO if it's mandatory)
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var phoneNumber = value.ToString();
            // Validates Vietnamese phone numbers (starts with 0 or +84, followed by 9 digits)
            var regex = new Regex(@"^(0|\+84)[3|5|7|8|9][0-9]{8}$");

            if (!regex.IsMatch(phoneNumber!))
            {
                return new ValidationResult(ErrorMessage ?? "Invalid phone number format.");
            }

            return ValidationResult.Success;
        }
    }
}