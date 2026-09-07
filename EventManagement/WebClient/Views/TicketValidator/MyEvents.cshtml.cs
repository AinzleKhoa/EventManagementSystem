using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Response;
using WebClient.Services;

namespace WebClient.Views.TicketValidator
{
    public class MyEventsModel : PageModel
    {
        private readonly IEventValidatorService _validatorService;
        private readonly IConfiguration _configuration;

        public MyEventsModel(IEventValidatorService validatorService, IConfiguration configuration)
        {
            _validatorService = validatorService;
            _configuration = configuration;
        }

        public List<ReadEventDTO> ValidatorEvents { get; set; } = new();
        public string ValidatorEmail { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get email from claim - in this system email is stored in "unique_name"
                ValidatorEmail = User.FindFirst("unique_name")?.Value;

                if (string.IsNullOrWhiteSpace(ValidatorEmail))
                {
                    ValidatorEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                }

                if (string.IsNullOrWhiteSpace(ValidatorEmail))
                {
                    ErrorMessage = "❌ Email not found. Please log in again.";
                    return Page();
                }

                // Load events
                ValidatorEvents = await _validatorService.GetEventsByValidatorEmailAsync(ValidatorEmail);

                if (ValidatorEvents == null || ValidatorEvents.Count == 0)
                {
                    ErrorMessage = "ℹ️ You have not been assigned to manage any events yet.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ Error: {ex.Message}";
            }

            return Page();
        }
    }
}