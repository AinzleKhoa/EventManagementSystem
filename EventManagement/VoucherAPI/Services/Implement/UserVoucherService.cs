using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using VoucherAPI.DTOs;
using VoucherAPI.Models;
using VoucherAPI.Repositories;
using VoucherAPI.Services;

namespace VoucherAPI.Services.Implement
{
    public class UserVoucherService : IUserVoucherService
    {
        private readonly IUserVoucherRepository _repo;
        IMapper _mapper;
        public UserVoucherService(IUserVoucherRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public IQueryable<UserVoucherReadDTO> GetAll()
        {
            var list = _repo.GetAll().ProjectTo<UserVoucherReadDTO>(_mapper.ConfigurationProvider);
            return list;
        }
        public async Task<UserVoucherReadDTO?> GetByIdAsync(int id)
            => _mapper.Map<UserVoucherReadDTO>(await _repo.GetByIdAsync(id));

        public async Task AddAsync(UserVoucherAddDTO voucher)
        {
            await _repo.AddAsync(_mapper.Map<UserVoucher>(voucher));
        }

        public async Task UpdateAsync(UserVoucherUpdateDTO voucher)
        {
            var userVoucher = await _repo.GetByIdAsync(voucher.Id);
            _mapper.Map(voucher, userVoucher);
            if (userVoucher.Quantity < 0)
            {
                await _repo.DeleteAsync(userVoucher);
            }
            else
            {
                await _repo.UpdateAsync(userVoucher);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var userVoucher = await _repo.GetByIdAsync(id);
            await _repo.DeleteAsync(userVoucher);
        }
    }
}
