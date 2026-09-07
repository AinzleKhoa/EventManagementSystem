using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VoucherAPI.Validations
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var password = value.ToString()!;

            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long.");
            }

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$");

            if (!regex.IsMatch(password))
            {
                return new ValidationResult(ErrorMessage ?? "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }

            return ValidationResult.Success;
        }
    }
}