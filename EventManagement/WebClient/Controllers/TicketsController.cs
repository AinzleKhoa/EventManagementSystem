using Microsoft.AspNetCore.Mvc;
using WebClient.Services;
public class TicketsController : Controller
{
    private readonly ITicketApiClient _ticketService;

    public TicketsController(ITicketApiClient ticketService)
    {
        _ticketService = ticketService;
    }

    // Action xử lý khi nhấn nút "View Tickets"
    public async Task<IActionResult> Details(int eventId)
    {
        // Gọi Service (đã được cấu hình AuthTokenHandler để gửi kèm Token)
        var tickets = await _ticketService.GetTicketsByEventIdAsync(eventId);

        ViewData["EventId"] = eventId;
        return View(tickets);
    }
}