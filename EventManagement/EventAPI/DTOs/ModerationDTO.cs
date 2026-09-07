using System.ComponentModel.DataAnnotations;
using EventAPI.CustomValidation;

namespace EventAPI.DTOs
{
    public class RejectEventDTO
    {
        [RejectReasonValidation]
        public string Reason { get; set; }
    }
}
