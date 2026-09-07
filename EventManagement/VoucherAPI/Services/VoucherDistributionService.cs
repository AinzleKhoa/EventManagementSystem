using System;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace VoucherAPI.Services
{
    public class VoucherDistributionService : IVoucherDistributionService
    {
        private readonly IUserVoucherService _userVoucherService;
        private readonly IVoucherService _voucherService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public VoucherDistributionService(IUserVoucherService userVoucherService, IVoucherService voucherService, HttpClient httpClient, IConfiguration configuration)
        {
            _userVoucherService = userVoucherService;
            _voucherService = voucherService;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task DistributeToUserByEmail(VoucherDistributeByEmailDTO dto)
        {
            var voucher = await _voucherService.GetByIdAsync(dto.VoucherId);

            if (voucher == null)
            {
                throw new InvalidOperationException("Voucher does not exist in the system.");
            }

            if (!(voucher.IsActive ?? false))
            {
                throw new InvalidOperationException("Cannot distribute. This voucher is currently inactive.");
            }

            if (voucher.UsedCount >= voucher.Quantity)
            {
                throw new InvalidOperationException("Cannot distribute. This voucher is out of stock.");
            }

            var userResponse = await _httpClient.GetAsync($"{_configuration["ApiSettings:AuthenticationApiUrl"]}/api/Users/search?SearchTerm={dto.UserEmail}");

            if (!userResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Error connecting to the Authentication API.");
            }

            var users = await userResponse.Content.ReadFromJsonAsync<PaginatedResultDTO<AppUser>>();
            var user = users?.Items?.FirstOrDefault();

            if (user == null)
            {
                throw new InvalidOperationException($"No user found with email: {dto.UserEmail}");
            }

            var userVoucher = new UserVoucherAddDTO
            {
                UserId = user.UserId,
                VoucherId = dto.VoucherId,
                Quantity = 1
            };

            await _userVoucherService.AddAsync(userVoucher);
        }
    }
}