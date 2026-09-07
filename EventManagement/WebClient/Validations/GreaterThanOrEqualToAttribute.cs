using System.ComponentModel.DataAnnotations;

namespace WebClient.Validations
{
    public class GreaterThanOrEqualToAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public GreaterThanOrEqualToAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

         
            var currentValue = (int)value;
            var propertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (propertyInfo == null)
            {
                return new ValidationResult($"Property {_comparisonProperty} not found.");
            }


            var comparisonValue = (int)propertyInfo.GetValue(validationContext.ObjectInstance)!;

            if (currentValue < comparisonValue)
            {
                var errorMessage = ErrorMessage ?? $"{validationContext.DisplayName} cannot be less than {comparisonValue}.";
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}