using System.ComponentModel.DataAnnotations;

namespace EventAPI.CustomValidation
{
    public class EventEditableStatusValidation
    {
        private static readonly string[] ApprovedStatuses = { "Approved", "Published" };
        public static bool IsEditable(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return true;

            return !ApprovedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsApproved(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return false;

            return ApprovedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        public static void ThrowIfNotEditable(string? status, string action = "edit", bool allowNonSensitiveForApproved = false)
        {
            if (allowNonSensitiveForApproved && IsApproved(status))
            {
                return;
            }

            if (!IsEditable(status))
            {
                throw new InvalidOperationException(
                    $"Cannot {action} this event because it has been approved. Current status: {status}.");
            }
        }
    }
}
