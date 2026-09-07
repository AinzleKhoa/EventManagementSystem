using WebClient.Api;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class UserVoucherService : IUserVoucherService
    {
        private readonly HttpClient _http;

        public UserVoucherService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ReadUserVoucherDTO>?> GetByUserId(int userId, int eventId)
        {
            var userVouchers = new List<ReadUserVoucherDTO>();

            // 1. Get vouchers assigned to user
            try
            {
                var uvList = await _http.GetFromJsonAsync<List<ReadUserVoucherDTO>>(
                    VoucherAPI.UserVoucherBase + $"?$filter=UserId eq {userId} and Quantity gt 0"
                );
                if (uvList != null)
                {
                    userVouchers.AddRange(uvList.Where(uv => uv.Voucher != null && (uv.Voucher.EventId == null || uv.Voucher.EventId == eventId)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching assigned user vouchers: {ex.Message}");
            }

            // 2. Get active event/global vouchers
            try
            {
                var eventVouchers = await _http.GetFromJsonAsync<List<ReadVoucherDTO>>(
                    VoucherAPI.Base + "?$filter=IsActive eq true"
                ) ?? new List<ReadVoucherDTO>();

                var existingVoucherIds = userVouchers
                    .Where(uv => uv.Voucher != null)
                    .Select(uv => uv.Voucher.VoucherId)
                    .ToHashSet();

                var nowUtc = DateTime.UtcNow;
                var nowLocal = DateTime.Now;

                foreach (var v in eventVouchers)
                {
                    bool matchesEvent = (v.EventId == null || v.EventId == eventId);

                    // Check date validity with tolerance for timezone (UTC vs Local)
                    bool validFromOk = !v.ValidFrom.HasValue || v.ValidFrom.Value <= nowUtc || v.ValidFrom.Value <= nowLocal || v.ValidFrom.Value.Date <= nowLocal.Date;
                    bool validToOk = !v.ValidTo.HasValue || v.ValidTo.Value >= nowUtc || v.ValidTo.Value >= nowLocal || v.ValidTo.Value.Date >= nowLocal.Date;

                    if (v.IsActive
                        && matchesEvent
                        && (v.Quantity > v.UsedCount)
                        && validFromOk
                        && validToOk
                        && !existingVoucherIds.Contains(v.VoucherId))
                    {
                        userVouchers.Add(new ReadUserVoucherDTO
                        {
                            Id = 0,
                            VoucherId = v.VoucherId,
                            UserId = userId,
                            Quantity = v.Quantity - v.UsedCount,
                            Voucher = v
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event/global vouchers: {ex.Message}");
            }

            return userVouchers;
        }
    }
}
