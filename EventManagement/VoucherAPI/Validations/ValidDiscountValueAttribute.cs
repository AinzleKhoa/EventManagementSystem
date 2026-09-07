using System;
using System.ComponentModel.DataAnnotations;

namespace VoucherAPI.Validations
{
    public class ValidDiscountValueAttribute : ValidationAttribute
    {
        private readonly string _discountTypeProperty;
        private readonly decimal _maxAmount;

        public ValidDiscountValueAttribute(string discountTypeProperty, double maxAmount = 100000000)
        {
            _discountTypeProperty = discountTypeProperty;
            _maxAmount = (decimal)maxAmount;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!decimal.TryParse(value.ToString(), out decimal discountValue))
            {
                return new ValidationResult("Invalid discount value format.");
            }

            var propertyInfo = validationContext.ObjectType.GetProperty(_discountTypeProperty);
            if (propertyInfo == null) return ValidationResult.Success;

            var discountType = propertyInfo.GetValue(validationContext.ObjectInstance) as string;

            if (string.IsNullOrWhiteSpace(discountType)) return ValidationResult.Success;

            if (string.Equals(discountType, "Percent", StringComparison.OrdinalIgnoreCase))
            {
                // Thay đổi thành < 1 để chặn tuyệt đối giá trị < 1%
                if (discountValue < 1 || discountValue > 100)
                {
                    return new ValidationResult(ErrorMessage ?? "Percentage discount must be between 1% and 100%.");
                }
            }
            else if (string.Equals(discountType, "Amount", StringComparison.OrdinalIgnoreCase))
            {
                if (discountValue <= 0)
                {
                    return new ValidationResult("Fixed amount discount must be greater than 0.");
                }

                if (discountValue > _maxAmount)
                {
                    return new ValidationResult($"Fixed amount discount cannot exceed {_maxAmount:N0} VND.");
                }
            }

            return ValidationResult.Success;
        }
    }
}