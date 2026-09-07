using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class RejectReasonValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Rejection reason is required.");

            var reason = value.ToString()!.Trim();

            if (reason.Length < 10)
                return new ValidationResult("Rejection reason must be at least 10 characters.");

            if (reason.Length > 1000)
                return new ValidationResult("Rejection reason cannot exceed 1000 characters.");

            return ValidationResult.Success;
        }
    }
}
