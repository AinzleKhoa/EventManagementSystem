using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebClient.Validations
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var phoneNumber = value.ToString();
            var regex = new Regex(@"^(0|\+84)[3|5|7|8|9][0-9]{8}$");

            if (!regex.IsMatch(phoneNumber!))
            {
                return new ValidationResult(ErrorMessage ?? "Invalid phone number format.");
            }

            return ValidationResult.Success;
        }
    }
}