using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using BookingAPI.DTOs.EventDTO;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.TicketTypeDTO;
using BookingAPI.DTOs.UserDTO;
using BookingAPI.DTOs.VoucherDTO;
using BookingAPI.Helpers;
using BookingAPI.Models;
using BookingAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.Json;

namespace BookingAPI.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly HttpClient _http;
        private readonly IChatApiClient _chatApiClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository repository,
            IMapper mapper,
            IHttpClientFactory factory,
            IConfiguration config,
            IChatApiClient chatApiClient,
            ILogger<OrderService> logger,
            IEmailService emailService)
        {
            _repository = repository;
            _mapper = mapper;
            _http = factory.CreateClient("GatewayAPI");
            _chatApiClient = chatApiClient;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task<int> Add(CreateOrderDTO model)
        {
            var newOrder = _mapper.Map<Order>(model);

            try
            {
                var userResponse = await _http.GetAsync($"auth-gateway/Users/{newOrder.CustomerId}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var user = await userResponse.Content.ReadFromJsonAsync<ReadUserDTO>();
                    if (user != null && user.IsActive != true)
                    {
                        throw new InvalidOperationException("Account has been locked by Administrator. Cannot create booking.");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not verify user active status: {ex.Message}");
            }

            var eventInfo = await ValidateEventAsync(newOrder.EventId);

            long totalAmount = CalculateOrderTotal(newOrder, eventInfo);

            long discountAmount = await CalculateDiscountAsync(model.VoucherId, totalAmount, newOrder.EventId);

            await MarkUserVoucherAsUsedAsync(model.UserVoucherId);

            FinalizeOrderData(newOrder, eventInfo, totalAmount, discountAmount);

            if (totalAmount == 0)
            {
                var tickets = await GenerateTickets(newOrder);
                newOrder.Tickets = tickets;

                await _repository.Add(newOrder);

                await DeductQuantity(newOrder);
                await SendEmailTickets(newOrder);
                await SendOrderPaidWebhookAsync(_mapper.Map<ReadOrderDTO>(newOrder));
                await SendPaymentSuccessNotification(newOrder.CustomerId, newOrder.EventName, newOrder.FinalAmount);
                await AddUserToEventChatAsync(newOrder.EventId, newOrder.CustomerId);
            }
            else
            {
                await _repository.Add(newOrder);
                await DeductQuantity(newOrder);
            }

            return newOrder.OrderId;
        }

        /// <summary>
        /// Add user to event chat after booking
        /// The conversation is already created when the event was created
        /// This method only adds the newly booked user as a participant
        /// </summary>
        private async Task AddUserToEventChatAsync(int eventId, int userId)
        {
            try
            {
                _logger.LogInformation($"[CHAT] Adding user to existing chat: eventId={eventId}, userId={userId}");

                var conversation = await _chatApiClient.GetOrCreateEventConversationAsync(eventId);

                if (conversation != null)
                {
                    _logger.LogInformation($"[CHAT] Found conversation {conversation.ConversationId}, adding participant {userId}");
                    await _chatApiClient.AddParticipantAsync(conversation.ConversationId, userId);
                    _logger.LogInformation($"[CHAT] ✅ SUCCESS: User {userId} added to conversation {conversation.ConversationId} for event {eventId}");
                    return;
                }

                var eventInfo = await _http.GetFromJsonAsync<ReadEventDTO>($"event-gateway/Events/{eventId}");

                if (eventInfo?.ConversationId != null)
                {
                    _logger.LogInformation($"[CHAT] Found event conversation {eventInfo.ConversationId}, adding participant {userId}");
                    await _chatApiClient.AddParticipantAsync(eventInfo.ConversationId.Value, userId);
                    _logger.LogInformation($"[CHAT] ✅ SUCCESS: User {userId} added to conversation {eventInfo.ConversationId} for event {eventId}");
                }
                else
                {
                    _logger.LogWarning($"[CHAT] ⚠️ WARNING: Event {eventId} has no associated conversation.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CHAT] ❌ EXCEPTION: Failed to add user {userId} to event chat for event {eventId}. Exception: {ex.Message}");
            }
        }

        public Task<List<Ticket>> GenerateTickets(Order order)
        {
            List<Ticket> tickets = new List<Ticket>();

            foreach (var detail in order.OrderDetails)
            {
                for (int i = 0; i < detail.Quantity; i++)
                {
                    tickets.Add(new Ticket
                    {
                        EventId = order.EventId,
                        OwnerName = order.FullName,
                        TicketTypeId = detail.TicketTypeId,
                        OrderId = order.OrderId
                    });
                }
            }

            return Task.FromResult(tickets);
        }
        public async Task DeductQuantity(Order order)
        {
            foreach (var detail in order.OrderDetails)
            {
               var response = await _http.PutAsJsonAsync(
                     $"event-gateway/TicketTypes/update-quantity/{detail.TicketTypeId}",
                     new UpdateTicketTypeQuantityDTO
                     {
                         Quantity = detail.Quantity
                     });
                Console.WriteLine("🔥 STATUS: " + response.StatusCode);
            }
        }
        public async Task Delete(int id)
        {
            await _repository.Delete(id);
        }

        public IQueryable<ReadOrderDTO> GetAll()
        {
            var list = _repository.GetAll().ProjectTo<ReadOrderDTO>(_mapper.ConfigurationProvider);
            return list;
        }

        public async Task<ReadOrderDTO> GetById(int id)
        {

            var order = _mapper.Map<ReadOrderDTO>(await _repository.GetById(id));
            var events = await _http.GetFromJsonAsync<ReadEventDTO>(
                           $"event-gateway/Events/{order.EventId}"
                       );
            foreach (var item in order.Tickets)
            {
                var ticket = events.TicketTypes.FirstOrDefault(x => x.TicketTypeId == item.TicketTypeId);
                item.TicketType = ticket;
            }
            foreach (var item in order.OrderDetails)
            {
                var ticket = events.TicketTypes.FirstOrDefault(x => x.TicketTypeId == item.TicketTypeId);
                item.TicketType = ticket;
            }
            return order;
        }

        public async Task Update(int id, Order model)
        {
            await _repository.Update(model);
        }


        private async Task<ReadEventDTO> ValidateEventAsync(int eventId)
        {
            var eventInfo = await _http.GetFromJsonAsync<ReadEventDTO>($"event-gateway/Events/{eventId}");

            if (eventInfo == null) throw new Exception("Event not found!");

            if (!eventInfo.Status.Equals("Approved"))
            {
                throw new Exception("This event is not approved to sell ticket!");
            }

            if (eventInfo.EndDate < DateTime.UtcNow)
            {
                throw new Exception("This event ended!");
            }

            return eventInfo;
        }

        private long CalculateOrderTotal(Order order, ReadEventDTO eventInfo)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any()) return 0;
            if (eventInfo.TicketTypes == null) throw new Exception("Ticket type null");

            long total = 0;

            foreach (var detail in order.OrderDetails)
            {
                var ticketType = eventInfo.TicketTypes.FirstOrDefault(x => x.TicketTypeId == detail.TicketTypeId);

                if (ticketType == null)
                    throw new Exception($"TicketType ID {detail.TicketTypeId} not found in event.");

                if (ticketType.Quantity - detail.Quantity < 0)
                {
                    throw new Exception($"Quantity of {ticketType.TypeName} is not enough!");
                }

                detail.UnitPrice = ticketType.Price;
                total += ticketType.Price * detail.Quantity;
            }

            return total;
        }

        private async Task<long> CalculateDiscountAsync(int? voucherId, long totalAmount, int eventId)
        {
            if (!voucherId.HasValue || voucherId.Value <= 0) return 0;

            if (totalAmount == 0 && voucherId.HasValue)
            {
                throw new Exception("You cannot use voucher for free tickets");
            }

            var voucher = await _http.GetFromJsonAsync<ReadVoucherDTO>($"voucher-gateway/Vouchers/{voucherId}");
            if (voucher == null) throw new Exception("Voucher is not valid");
            if (voucher.EventId != eventId) throw new Exception("Voucher is not for this event");
            long discountAmount = 0;
            if ("Percent".Equals(voucher.DiscountType, StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = (long)(totalAmount * voucher.DiscountValue) / 100;
            }
            else if ("Amount".Equals(voucher.DiscountType, StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = (long)voucher.DiscountValue;
            }

            return discountAmount;
        }

        public async Task<bool> HasPaidTicketAsync(int customerId, int eventId)
        {
            var hasPaid = await _repository.GetAll()
                .AnyAsync(o => o.CustomerId == customerId
                            && o.EventId == eventId
                            && o.Status == "Paid");
            return hasPaid;
        }

        private async Task MarkUserVoucherAsUsedAsync(int? userVoucherId)
        {
            if (!userVoucherId.HasValue || userVoucherId.Value <= 0) return;

            var response = await _http.PutAsync($"voucher-gateway/UserVouchers/use/{userVoucherId}", null);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error using voucher {userVoucherId}: {response.ReasonPhrase}");
            }
        }

        private void FinalizeOrderData(Order order, ReadEventDTO eventInfo, long totalAmount, long discountAmount)
        {
            order.DiscountAmount = discountAmount;
            order.TotalAmount = totalAmount;
            order.FinalAmount = totalAmount - discountAmount;
            order.EventName = eventInfo.Title;
            order.PosterUrl = eventInfo.PosterUrl;
            order.StartDate = eventInfo.StartDate;
            order.EndDate = eventInfo.EndDate;
            order.Status = totalAmount == 0 ? "Paid" : "Pending";
            order.PaymentMethod = null;
            order.TransactionId = null;
            order.ExpiresAt = DateTime.UtcNow.AddMinutes(15);
            order.OrderDate = DateTime.UtcNow;
        }

        public async Task SendEmailTickets(Order order)
        {
            try
            {
                if (order == null)
                {
                    Console.WriteLine("SendEmailTickets: Order is null");
                    return;
                }

                if (order.Tickets == null || !order.Tickets.Any())
                {
                    Console.WriteLine($"SendEmailTickets: Order {order.OrderId} has no tickets");
                    return;
                }

                string htmlBody = $@"
                <div style='font-family:Arial;background:#f6f6f6;padding:30px'>
                <div style='max-width:600px;margin:auto;background:white;border-radius:10px;padding:25px'>

                <h2 style='color:#4CAF50'>🎟 Booking Confirmation</h2>

                <p>Xin chào <b>{order.FullName}</b>,</p>
                <p>Đơn hàng <b>#{order.OrderId}</b> của bạn đã được xác nhận thành công.</p>

                <hr/>
                <h3>Danh sách vé</h3>
                ";

                var resources = new List<LinkedResource>();
                int index = 0;

                foreach (var ticket in order.Tickets)
                {
                    try
                    {
                        if (ticket.TicketCode == Guid.Empty)
                            continue;

                        // 🔹 DATA QR giống Razor page
                        var qrData = new
                        {
                            ticketId = ticket.TicketId,
                            ticketCode = ticket.TicketCode,
                            attendee = ticket.OwnerName,
                            eventName = order.EventName,
                            orderId = order.OrderId
                        };

                        // 🔹 Serialize JSON
                        var qrContent = JsonSerializer.Serialize(qrData);

                        // 🔹 Generate QR từ JSON
                        var qrBytes = GenerateQr.GenerateQrBytes(qrContent);

                        var stream = new MemoryStream(qrBytes);

                        string contentId = $"qr{index}";

                        var resource = new LinkedResource(stream, "image/png")
                        {
                            ContentId = contentId
                        };

                        resources.Add(resource);

                        htmlBody += $@"
                        <div style='border:1px solid #eee;border-radius:8px;padding:15px;margin-bottom:15px'>

                        <p><b>Ticket Code:</b> {ticket.TicketCode}</p>
                        <p><b>Owner:</b> {ticket.OwnerName}</p>
                        <p><b>Status:</b> {ticket.Status}</p>

                        <div style='text-align:center;margin-top:10px'>
                        <img src=""cid:{contentId}"" style=""width:160px;height:160px""/>
                        </div>

                        </div>
                        ";

                        index++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"QR generation error for Ticket {ticket.TicketId}: {ex.Message}");
                    }
                }

                htmlBody += @"
                <hr/>

                <p style='font-size:13px;color:#777'>
                Vui lòng xuất trình QR Code tại cổng check-in của sự kiện.
                </p>

                <p style='font-size:13px;color:#999'>
                Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.
                </p>

                </div>
                </div>
                ";

                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody,
                    null,
                    MediaTypeNames.Text.Html
                );

                foreach (var resource in resources)
                {
                    htmlView.LinkedResources.Add(resource);
                }

                await _emailService.SendEmailAsync(
                    order.Email,
                    "Your Tickets 🎟",
                    htmlView
                );

                Console.WriteLine($"SendEmailTickets success for Order {order.OrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendEmailTickets ERROR Order {order?.OrderId}: {ex.Message}");
            }
        }
        public async Task<string> HandleOrderReturn(Order order, string paymentMethod, string? transactionId)
        {
            if (order.Status == "Paid")
                return "Order already paid";

            order.PaymentMethod = paymentMethod;
            order.TransactionId = transactionId;

            var tickets = await GenerateTickets(order);

            order.Tickets = tickets;

            order.Status = "Paid";

            await _repository.Update(order);

            await SendEmailTickets(order);
            await AddUserToEventChatAsync(order.EventId, order.CustomerId);
            return "Payment success";
        }
        
        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _repository.GetById(orderId);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";
                await _repository.Update(order);

                // Restore ticket quantities
                foreach (var detail in order.OrderDetails)
                {
                    await _http.PutAsJsonAsync(
                        $"event-gateway/TicketTypes/update-quantity/{detail.TicketTypeId}",
                        new UpdateTicketTypeQuantityDTO
                        {
                            Quantity = -detail.Quantity
                        });
                }

                // Restore voucher
                if (order.UserVoucherId.HasValue && order.UserVoucherId.Value > 0)
                {
                    await _http.PutAsync($"voucher-gateway/UserVouchers/restore/{order.UserVoucherId}", null);
                }
            }
        }
        public async Task SendOrderPaidWebhookAsync(ReadOrderDTO order)
        {
            try
            {
                if (order == null || (order.Status != "Paid" && order.Status != "Completed"))
                    return;

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                };

                using var httpClient = new HttpClient(handler);

                string notificationApiUrl =
                    $"https://localhost:7197/api/webhook/order-paid/{order.EventId}";

                var response = await httpClient.PostAsync(notificationApiUrl, null);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("[Success] Sent Webhook to NotificationAPI!");
                else
                    Console.WriteLine($"[Error WEBHOOK] Server return: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception WEBHOOK]: {ex.Message}");
            }
        }
        private async Task SendPaymentSuccessNotification(int userId, string eventName, long amount)
        {
            try
            {
                var notifPayload = new
                {
                    UserId = userId,
                    Title = "Payment Successful!",
                    Message = $"Your ticket order for the event {eventName} worth {amount:N0} VND has been successfully paid. Your tickets are now ready!",
                    Type = "Order"
                };

                var response = await _http.PostAsJsonAsync("notification-gateway/Notifications", notifPayload);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error sending payment success notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while sending notification: {ex.Message}");
            }
        }

    }
}
