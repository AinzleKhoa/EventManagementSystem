using System.ComponentModel.DataAnnotations;

namespace BookingAPI.CustomValidation
{
    public class MoneyValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is long amount)
            {
                if (amount < 0)
                    return new ValidationResult($"{validationContext.MemberName} cannot be negative");

                if (amount > 1_000_000_000)
                    return new ValidationResult($"{validationContext.MemberName} is too large");

                return ValidationResult.Success;
            }

            if (value is decimal || value is double || value is float)
            {
                return new ValidationResult($"{validationContext.MemberName} cannot contain decimal values");
            }

            return new ValidationResult("Invalid money format");
        }
    }
}
