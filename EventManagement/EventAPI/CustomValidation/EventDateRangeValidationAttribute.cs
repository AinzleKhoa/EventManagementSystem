using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventDateRangeValidationAttribute : ValidationAttribute
    {
        private readonly string _startDateProperty;
        private readonly string _endDateProperty;

        public EventDateRangeValidationAttribute(string startDateProperty = "StartDate", string endDateProperty = "EndDate")
        {
            _startDateProperty = startDateProperty;
            _endDateProperty = endDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var type = value.GetType();

            var startProp = type.GetProperty(_startDateProperty);
            var endProp = type.GetProperty(_endDateProperty);

            if (startProp == null || endProp == null)
                return ValidationResult.Success;

            var startValue = startProp.GetValue(value);
            var endValue = endProp.GetValue(value);

            if (startValue is not DateTime startDate || endValue is not DateTime endDate)
                return ValidationResult.Success;

            if (startDate == default || endDate == default)
                return new ValidationResult("Start Date and End Date are required.");

            if (endDate <= startDate)
                return new ValidationResult("End Date must be after Start Date.");

            var duration = endDate - startDate;
            if (duration.TotalMinutes < 30)
                return new ValidationResult("Event must be at least 30 minutes long.");

            if (duration.TotalDays > 365)
                return new ValidationResult("Event duration cannot exceed 365 days.");

            return ValidationResult.Success;
        }
    }
}
