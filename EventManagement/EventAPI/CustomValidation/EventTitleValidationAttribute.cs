using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class EventTitleValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Event title is required.");

            var title = value.ToString()!.Trim();

            if (title.Length < 3)
                return new ValidationResult("Event title must be at least 3 characters.");

            if (title.Length > 200)
                return new ValidationResult("Event title cannot exceed 200 characters.");

            if (!char.IsLetterOrDigit(title[0]))
                return new ValidationResult("Event title must start with a letter or digit.");

            return ValidationResult.Success;
        }
    }
}
