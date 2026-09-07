using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class LatitudeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; 

            if (value is double lat)
            {
                if (lat < -90.0 || lat > 90.0)
                    return new ValidationResult("Latitude must be between -90 and 90.");

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid latitude format.");
        }
    }

    public class LongitudeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; 

            if (value is double lng)
            {
                if (lng < -180.0 || lng > 180.0)
                    return new ValidationResult("Longitude must be between -180 and 180.");

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid longitude format.");
        }
    }
}
