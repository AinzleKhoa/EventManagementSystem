using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebClient.DTOs.Response;
using WebClient.Services;

namespace WebClient.Views.Events
{
    public class ValidatorsModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly IEventValidatorService _validatorService;

        public ValidatorsModel(IEventService eventService, IEventValidatorService validatorService)
        {
            _eventService = eventService;
            _validatorService = validatorService;
        }

        public ReadEventDTO? Event { get; set; }
        public List<EventValidatorDTO> Validators { get; set; } = new();

        [BindProperty]
        public string ValidatorEmail { get; set; }

        public int EventId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            EventId = id;
            try
            {
                Event = await _eventService.GetById(id);

                if (Event == null)
                {
                    return NotFound();
                }

                Validators = await _validatorService.GetValidatorsByEventIdAsync(id);
            }
            catch (Exception ex)
            {
                // Log error - return error page instead of crashing
                Console.WriteLine($"Error loading validators page: {ex.Message}");
                TempData["ToastMessage"] = "✗ Error: Unable to load the page. Please try again.";
                TempData["ToastType"] = "danger";
                return RedirectToPage("/Events/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddValidator(int id, string validatorEmail)
        {
            EventId = id;

            Console.WriteLine($"OnPostAddValidator called: EventId={id}, Email={validatorEmail}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"  - {error.ErrorMessage}");
                    }
                }
                return new JsonResult(new { success = false, message = "❌ Invalid data. Please check again." })
                {
                    StatusCode = 400,
                    ContentType = "application/json; charset=utf-8"
                };
            }

            if (string.IsNullOrWhiteSpace(validatorEmail))
            {
                return new JsonResult(new { success = false, message = "❌ Validator email cannot be empty." })
                {
                    StatusCode = 400,
                    ContentType = "application/json; charset=utf-8"
                };
            }

            if (!IsValidEmail(validatorEmail))
            {
                return new JsonResult(new { success = false, message = "❌ Invalid email format. Please check again." })
                {
                    StatusCode = 400,
                    ContentType = "application/json; charset=utf-8"
                };
            }

            try
            {
                validatorEmail = validatorEmail.Trim();
                Console.WriteLine($"Adding validator: EventId={id}, Email={validatorEmail}");
                var result = await _validatorService.AddValidatorAsync(id, validatorEmail);

                Console.WriteLine($"Validator added successfully: {result.StaffEmail}");

                Validators = await _validatorService.GetValidatorsByEventIdAsync(id);

                return new JsonResult(new { success = true, message = $"✓ Validator {validatorEmail} has been added successfully." })
                {
                    ContentType = "application/json; charset=utf-8"
                };
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"API error: {ex.Message}");
                string errorMessage = ex.Message;

                if (!errorMessage.StartsWith("❌"))
                {
                    if (errorMessage.Contains("đã được thêm") && errorMessage.Contains("rồi"))
                    {
                        errorMessage = $"❌ Validator {validatorEmail} has already been added to this event.";
                    }
                    else if (errorMessage.Contains("không tồn tại"))
                    {
                        errorMessage = "❌ Event does not exist.";
                    }
                    else if (errorMessage.Contains("không hợp lệ"))
                    {
                        errorMessage = "❌ Invalid email.";
                    }
                    else
                    {
                        errorMessage = $"❌ {errorMessage}";
                    }
                }

                return new JsonResult(new { success = false, message = errorMessage })
                {
                    StatusCode = 400,
                    ContentType = "application/json; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Add validator error: {ex.Message}\n{ex.StackTrace}");
                return new JsonResult(new { success = false, message = $"❌ Error: {ex.Message}" })
                {
                    StatusCode = 500,
                    ContentType = "application/json; charset=utf-8"
                };
            }
        }

        public async Task<IActionResult> OnPostRemoveValidator(int id, int eventStaffId)
        {
            EventId = id;

            Console.WriteLine($"OnPostRemoveValidator called: EventId={id}, EventStaffId={eventStaffId}");

            try
            {
                var success = await _validatorService.RemoveValidatorAsync(eventStaffId);

                if (success)
                {
                    Validators = await _validatorService.GetValidatorsByEventIdAsync(id);
                    return new JsonResult(new { success = true, message = "✓ Validator has been removed successfully." })
                    {
                        ContentType = "application/json; charset=utf-8"
                    };
                }
                else
                {
                    return new JsonResult(new { success = false, message = "❌ Validator not found." })
                    {
                        StatusCode = 400,
                        ContentType = "application/json; charset=utf-8"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Remove validator error: {ex.Message}");
                return new JsonResult(new { success = false, message = $"❌ Error: {ex.Message}" })
                {
                    StatusCode = 500,
                    ContentType = "application/json; charset=utf-8"
                };
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }
    }
}