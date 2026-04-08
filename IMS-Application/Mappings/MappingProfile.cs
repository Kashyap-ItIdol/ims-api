using AutoMapper;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TicketComment, TicketCommentResponseDto>()
                .ForMember(dest => dest.ticketId, opt => opt.MapFrom(src => src.TicketId.ToString()))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ReverseMap();

            CreateMap<Category, ListCategoriesDto>();
            CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.RoleId,
                       opt => opt.MapFrom(src => src.RoleId));
        }
    }
}
