using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Models;
using VoucherAPI.Repositories;

namespace VoucherAPI.Services.Implement
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _repo;
        private readonly IMapper _mapper;

        public VoucherService(IVoucherRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<Voucher>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Voucher?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Voucher> CreateAsync(VoucherCreateDTO dto)
        {

            var codeToCheck = dto.Code?.ToUpper().Trim();
            var isDuplicate = await _repo.GetQueryable().AnyAsync(v =>
                v.Code.ToUpper() == codeToCheck &&
                v.DiscountType == dto.DiscountType &&
                v.DiscountValue == dto.DiscountValue);

            if (isDuplicate)
            {
                throw new InvalidOperationException("Cannot be created. A duplicate voucher already exists with all three pieces of information: Code, Discount Type, and Discount Value.");
            }

            var voucher = _mapper.Map<Voucher>(dto);

            if (!string.IsNullOrEmpty(voucher.Code))
            {
                voucher.Code = voucher.Code.ToUpper().Trim();
            }
            voucher.UsedCount = 0;
            voucher.IsActive = true;

            await _repo.AddAsync(voucher);
            return voucher;
        }

        public async Task<bool> UpdateAsync(int id, VoucherUpdateDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            var codeToCheck = (dto.Code ?? existing.Code).ToUpper().Trim();
            var isDuplicate = await _repo.GetQueryable().AnyAsync(v =>
                v.VoucherId != id &&
                v.Code.ToUpper() == codeToCheck &&
                v.DiscountType == dto.DiscountType &&
                v.DiscountValue == dto.DiscountValue);

            if (isDuplicate)
            {
                throw new InvalidOperationException("Unable to update. Another voucher already exists with the same code, discount type, and discount value.");
            }

            if (existing.UsedCount > 0)
            {
                if (dto.Quantity < existing.UsedCount)
                {
                    throw new InvalidOperationException($"Cannot set quantity to {dto.Quantity} because {existing.UsedCount} vouchers have already been used.");
                }
                existing.Quantity = dto.Quantity;
                existing.ValidFrom = dto.ValidFrom;
                existing.ValidTo = dto.ValidTo;
                existing.IsActive = dto.IsActive;
            }
            else
            {
                _mapper.Map(dto, existing);
            }

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.UsedCount > 0)
            {
                existing.IsActive = false;
                await _repo.UpdateAsync(existing);
            }
            else
            {
                await _repo.DeleteAsync(existing);
            }

            return true;
        }

        public async Task<PaginatedResultDTO<VoucherReadDTO>> SearchAndFilter(VoucherSearchFilterDTO filter)
        {
            if (filter.PageNumber < 1)
            {
                filter.PageNumber = 1;
            }
            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                filter.PageSize = 10;
            }

            var (items, totalCount) = await _repo.SearchAndFilter(filter);

            return new PaginatedResultDTO<VoucherReadDTO>
            {
                Items = _mapper.Map<List<VoucherReadDTO>>(items),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public IQueryable<Voucher> GetQueryable()
        {
            return _repo.GetQueryable();
        }
    }
}