using AutoMapper;
using ChatAPI.Models;
using ChatAPI.DTOs;

namespace ChatAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Conversation, ConversationDTO>()
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count));

            CreateMap<CreateConversationDTO, Conversation>();
            CreateMap<UpdateConversationDTO, Conversation>();

            CreateMap<Message, MessageDTO>();
            CreateMap<CreateMessageDTO, Message>();
            CreateMap<UpdateMessageDTO, Message>();

            CreateMap<ConversationParticipant, ConversationParticipantDTO>();
            CreateMap<CreateConversationDTO, Conversation>();
        }
    }
}
