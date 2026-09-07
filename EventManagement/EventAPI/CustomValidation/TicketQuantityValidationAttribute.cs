using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class TicketQuantityValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Ticket quantity is required.");

            if (value is int quantity)
            {
                if (quantity < 1)
                    return new ValidationResult("Ticket quantity must be at least 1.");

                if (quantity > 100_000)
                    return new ValidationResult("Ticket quantity cannot exceed 100,000.");

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid ticket quantity format.");
        }
    }
}
