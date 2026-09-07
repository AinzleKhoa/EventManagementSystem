using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class EventDescriptionValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var description = value.ToString();

            if (string.IsNullOrWhiteSpace(description))
                return new ValidationResult("Description cannot be only whitespace.");

            if (description.Length > 5000)
                return new ValidationResult("Description cannot exceed 5000 characters.");

            return ValidationResult.Success;
        }
    }
}
