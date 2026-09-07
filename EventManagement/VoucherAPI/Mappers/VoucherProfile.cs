using AutoMapper;
using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Mappers
{
    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<Voucher, VoucherReadDTO>();
            CreateMap<VoucherCreateDTO, Voucher>();
            CreateMap<VoucherUpdateDTO, Voucher>();
            CreateMap<Voucher, VoucherUpdateDTO>();

            CreateMap<UserVoucher, UserVoucherReadDTO>();
            CreateMap<UserVoucherAddDTO, UserVoucher>();
            CreateMap<UserVoucherUpdateDTO, UserVoucher>();


        }
    }

}
