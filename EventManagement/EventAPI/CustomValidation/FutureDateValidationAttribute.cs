using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class FutureDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult($"{validationContext.DisplayName} is required.");

            if (value is DateTime dateTime)
            {
                if (dateTime == default)
                    return new ValidationResult($"{validationContext.DisplayName} is required.");

                if (dateTime <= DateTime.Now)
                    return new ValidationResult($"{validationContext.DisplayName} must be a future date.");

                if (dateTime > DateTime.Now.AddYears(5))
                    return new ValidationResult($"{validationContext.DisplayName} cannot be more than 5 years from now.");

                return ValidationResult.Success;
            }

            return new ValidationResult($"{validationContext.DisplayName} is not a valid date.");
        }
    }
}
