using BookingAPI.Repositories;
using BookingAPI.Services;

namespace BookingAPI.Background
{
    public class OrderBackgroundJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrderBackgroundJob(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        public async Task CancelExpiredOrders()
        {
            var orders = _orderRepository.GetAll().Where(o => o.Status.Equals("Pending")).ToList();

            foreach (var order in orders)
            {
                if (order.ExpiresAt.HasValue &&
                    DateTime.UtcNow > order.ExpiresAt)
                {
                    await _orderService.CancelOrderAsync(order.OrderId);
                }
            }
        }
    }
}
