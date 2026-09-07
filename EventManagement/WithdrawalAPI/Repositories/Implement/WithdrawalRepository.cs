using Microsoft.EntityFrameworkCore;
using WithdrawalAPI.DTOs;
using WithdrawalAPI.Models;

namespace WithdrawalAPI.Repositories.Implement
{
    public class WithdrawalRepository : IWithdrawalRepository
    {
        private readonly WithdrawalDbContext _context;

        public WithdrawalRepository(WithdrawalDbContext context)
        {
            _context = context;
        }

        public async Task Add(WithdrawalRequest model)
        {
            _context.WithdrawalRequests.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task<WithdrawalRequest?> GetById(int id)
        {
            return await _context.WithdrawalRequests.FindAsync(id);
        }

        public async Task<List<WithdrawalRequest>> GetByOrganizerId(int organizerId)
        {
            return await _context.WithdrawalRequests
                .Where(w => w.OrganizerId == organizerId)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
        }

        public async Task Update(WithdrawalRequest model)
        {
            _context.WithdrawalRequests.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<WithdrawalRequest> Items, int TotalCount)> SearchAndFilter(WithdrawalFilterDTO filter)
        {
            var query = _context.WithdrawalRequests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(w =>
                    w.BankName.ToLower().Contains(term) ||
                    w.BankAccountNumber.ToLower().Contains(term) ||
                    w.BankAccountName.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(w => w.Status == filter.Status);
            }

            if (filter.OrganizerId.HasValue)
            {
                query = query.Where(w => w.OrganizerId == filter.OrganizerId.Value);
            }

            var totalCount = await query.CountAsync();

            query = (filter.SortBy?.ToLower(), filter.SortOrder?.ToLower()) switch
            {
                ("amount", "asc")        => query.OrderBy(w => w.Amount),
                ("amount", "desc")       => query.OrderByDescending(w => w.Amount),
                ("status", "asc")        => query.OrderBy(w => w.Status),
                ("status", "desc")       => query.OrderByDescending(w => w.Status),
                ("processedat", "asc")   => query.OrderBy(w => w.ProcessedAt),
                ("processedat", "desc")  => query.OrderByDescending(w => w.ProcessedAt),
                (_, "asc")               => query.OrderBy(w => w.RequestedAt),
                _                        => query.OrderByDescending(w => w.RequestedAt)
            };

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize   = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> CanEdit(int id)
        {
            var w = await _context.WithdrawalRequests.FindAsync(id);
            return w != null && w.Status == "Pending";
        }

        public IQueryable<WithdrawalRequest> GetQueryable()
        {
            return _context.WithdrawalRequests.AsQueryable();
        }
    }
}
