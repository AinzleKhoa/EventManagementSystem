using System;
using System.ComponentModel.DataAnnotations;

namespace WebClient.Validations
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var currentValue = (DateTime)value;
            var propertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Property with name {_comparisonProperty} not found.");
            }

            var comparisonValue = propertyInfo.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (comparisonValue.HasValue && currentValue.Date < comparisonValue.Value.Date)
            {
                return new ValidationResult(ErrorMessage ?? $"End date must be on or after {_comparisonProperty}.");
            }

            return ValidationResult.Success;
        }
    }
}