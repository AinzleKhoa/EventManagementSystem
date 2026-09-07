using Microsoft.EntityFrameworkCore;
using System;
using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Repositories.Implement
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly VoucherDbContext _context;

        public VoucherRepository(VoucherDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetAllAsync()
            => await _context.Vouchers.ToListAsync();

        public async Task<Voucher?> GetByIdAsync(int id)
            => await _context.Vouchers.FindAsync(id);

        public async Task AddAsync(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Voucher voucher)
        {
            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Voucher voucher)
        {
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Voucher> Items, int TotalCount)> SearchAndFilter(VoucherSearchFilterDTO filter)
        {
            try
            {
                var query = _context.Vouchers.AsQueryable();

                if (filter.EventIds != null && filter.EventIds.Any())
                {
                    query = query.Where(c => filter.EventIds.Contains(c.EventId));
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(c => c.Code.ToLower().Contains(searchTerm) ||
                                           (c.DiscountType != null && c.DiscountType.ToLower().Contains(searchTerm)));
                }
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == filter.IsActive.Value);
                }

                var totalCount = await query.CountAsync();

                query = filter.SortBy?.ToLower() switch
                {
                    "discounttype" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.DiscountType)
                        : query.OrderBy(c => c.DiscountType),
                    "isactive" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.IsActive)
                        : query.OrderBy(c => c.IsActive),
                    _ => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.Code)
                        : query.OrderBy(c => c.Code)
                };

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public IQueryable<Voucher> GetQueryable()
        {
            return _context.Vouchers.AsNoTracking();
        }

    }

}
