using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class TicketTypeNameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Ticket type name is required.");

            var name = value.ToString()!.Trim();

            if (name.Length < 1)
                return new ValidationResult("Ticket type name must be at least 1 character.");

            if (name.Length > 100)
                return new ValidationResult("Ticket type name cannot exceed 100 characters.");

            if (!char.IsLetterOrDigit(name[0]))
                return new ValidationResult("Ticket type name must start with a letter or digit.");

            return ValidationResult.Success;
        }
    }
}
