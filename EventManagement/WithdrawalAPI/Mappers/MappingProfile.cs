using AutoMapper;
using WithdrawalAPI.DTOs;
using WithdrawalAPI.Models;

namespace WithdrawalAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<WithdrawalRequest, ReadWithdrawalDTO>();
            CreateMap<CreateWithdrawalDTO, WithdrawalRequest>();
            CreateMap<UpdateWithdrawalDTO, WithdrawalRequest>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectReason, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizerId, opt => opt.Ignore());
        }
    }
}
