using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using WithdrawalAPI.DTOs;
using WithdrawalAPI.Models;
using WithdrawalAPI.Repositories;

namespace WithdrawalAPI.Services.Implement
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IWithdrawalRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEventApiClient _eventApiClient;
        private readonly IBookingApiClient _bookingApiClient;

        public WithdrawalService(
            IWithdrawalRepository repo,
            IMapper mapper,
            IEventApiClient eventApiClient,
            IBookingApiClient bookingApiClient)
        {
            _repo = repo;
            _mapper = mapper;
            _eventApiClient = eventApiClient;
            _bookingApiClient = bookingApiClient;
        }

        public async Task<ReadWithdrawalDTO> Create(int organizerId, CreateWithdrawalDTO model)
        {
            if (model.EventId.HasValue)
            {
                // 1. Verify event existence and ownership
                var eventDetails = await _eventApiClient.GetEventByIdAsync(model.EventId.Value);
                if (eventDetails == null)
                {
                    throw new Exception($"Event with ID {model.EventId.Value} does not exist.");
                }

                if (eventDetails.OrganizerId != organizerId)
                {
                    throw new Exception("You do not have permission to withdraw funds for this event.");
                }

                // 2. Verify sufficient revenue from BookingAPI (after 10% platform fee)
                var orders = await _bookingApiClient.GetOrdersByEventIdAsync(model.EventId.Value);
                decimal totalRevenue = orders
                    .Where(o => o.Status == "Paid" || o.Status == "Completed")
                    .Sum(o => (decimal)o.FinalAmount);
                decimal maxWithdrawable = totalRevenue * 0.9m;

                // 3. Subtract already withdrawn or pending amounts
                decimal existingWithdrawnAmount = _repo.GetQueryable()
                    .Where(w => w.EventId == model.EventId.Value && (w.Status == "Pending" || w.Status == "Approved"))
                    .Sum(w => w.Amount);

                if (existingWithdrawnAmount + model.Amount > maxWithdrawable)
                {
                    throw new Exception($"Insufficient funds. Max net revenue allowed to withdraw (after 10% platform fee): {maxWithdrawable}, Already withdrawn/pending: {existingWithdrawnAmount}, Requested: {model.Amount}");
                }
            }

            var entity = _mapper.Map<WithdrawalRequest>(model);
            entity.OrganizerId = organizerId;
            entity.Status = "Pending";
            entity.RequestedAt = DateTime.Now;

            await _repo.Add(entity);
            return _mapper.Map<ReadWithdrawalDTO>(entity);
        }

        public async Task<ReadWithdrawalDTO> Update(int id, int organizerId, UpdateWithdrawalDTO model)
        {
            var entity = await _repo.GetById(id)
                ?? throw new Exception("Withdrawal request not found.");

            if (entity.OrganizerId != organizerId)
                throw new Exception("You do not have permission to update this request.");

            if (entity.Status != "Pending")
                throw new Exception("Only pending requests can be updated.");

            _mapper.Map(model, entity);
            await _repo.Update(entity);
            return _mapper.Map<ReadWithdrawalDTO>(entity);
        }

        public async Task<List<ReadWithdrawalDTO>> GetByOrganizer(int organizerId)
        {
            var items = await _repo.GetByOrganizerId(organizerId);
            return _mapper.Map<List<ReadWithdrawalDTO>>(items);
        }

        public async Task<ReadWithdrawalDTO> GetById(int id)
        {
            var entity = await _repo.GetById(id)
                ?? throw new Exception("Withdrawal request not found.");
            return _mapper.Map<ReadWithdrawalDTO>(entity);
        }

        public async Task<PaginatedResultDTO<ReadWithdrawalDTO>> SearchAndFilter(WithdrawalFilterDTO filter)
        {
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.PageSize > 100) filter.PageSize = 100;

            var (items, totalCount) = await _repo.SearchAndFilter(filter);

            return new PaginatedResultDTO<ReadWithdrawalDTO>
            {
                Items = _mapper.Map<List<ReadWithdrawalDTO>>(items),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ReadWithdrawalDTO> Approve(int id)
        {
            var entity = await _repo.GetById(id)
                ?? throw new Exception("Withdrawal request not found.");

            if (entity.Status != "Pending")
                throw new Exception("Only pending requests can be approved.");

            entity.Status = "Approved";
            entity.ProcessedAt = DateTime.Now;

            await _repo.Update(entity);
            return _mapper.Map<ReadWithdrawalDTO>(entity);
        }

        public async Task<ReadWithdrawalDTO> Reject(int id, RejectReasonDTO dto)
        {
            var entity = await _repo.GetById(id)
                ?? throw new Exception("Withdrawal request not found.");

            if (entity.Status != "Pending")
                throw new Exception("Only pending requests can be rejected.");

            entity.Status = "Rejected";
            entity.ProcessedAt = DateTime.Now;
            entity.RejectReason = dto.RejectReason;

            await _repo.Update(entity);
            return _mapper.Map<ReadWithdrawalDTO>(entity);
        }

        public IQueryable<ReadWithdrawalDTO> GetAllQueryable()
            => _repo.GetQueryable().ProjectTo<ReadWithdrawalDTO>(_mapper.ConfigurationProvider);
    }
}
