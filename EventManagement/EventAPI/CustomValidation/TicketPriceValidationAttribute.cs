using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class TicketPriceValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is long price)
            {
                if (price < 0)
                    return new ValidationResult("Ticket price cannot be negative.");

                if (price > 100_000_000)
                    return new ValidationResult("Ticket price cannot exceed 100,000,000.");

                return ValidationResult.Success;
            }

            if (value is decimal || value is double || value is float)
                return new ValidationResult("Ticket price must be a whole number.");

            return new ValidationResult("Invalid ticket price format.");
        }
    }
}
