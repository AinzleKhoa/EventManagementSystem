using System.ComponentModel.DataAnnotations;

namespace VoucherAPI.Validations
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime.Date < DateTime.Now.Date)
                {
                    return new ValidationResult(ErrorMessage ?? "Date cannot be in the past.");
                }
            }
            return ValidationResult.Success;
        }
    }
}