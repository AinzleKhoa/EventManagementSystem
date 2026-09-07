using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class EventLocationValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var location = value.ToString();

            if (string.IsNullOrWhiteSpace(location))
                return new ValidationResult("Location cannot be only whitespace.");

            location = location.Trim();

            if (location.Length < 2)
                return new ValidationResult("Location must be at least 2 characters.");

            if (location.Length > 500)
                return new ValidationResult("Location cannot exceed 500 characters.");

            return ValidationResult.Success;
        }
    }
}
