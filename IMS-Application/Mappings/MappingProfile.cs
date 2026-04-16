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

            CreateMap<Category, GetCategoryDto>();
            CreateMap<SubCategory, SubCategoryDto>();

            // TicketService mappings
            CreateMap<CreateTicketRequestDto, Ticket>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TicketType, opt => opt.MapFrom(src => (TicketType)Enum.Parse(typeof(TicketType), src.TicketType)))
                .ForMember(dest => dest.TicketPriority, opt => opt.MapFrom(src => (TicketPriority)Enum.Parse(typeof(TicketPriority), src.Priority)))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId));

            CreateMap<Ticket, TicketInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => $"TKT-{src.Id}"))
                .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TicketType, opt => opt.MapFrom(src => src.TicketType.ToString()))
                .ForMember(dest => dest.TicketPriority, opt => opt.MapFrom(src => src.TicketPriority.ToString()))
                .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.updatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.createdBy, opt => opt.Ignore())
                .ForMember(dest => dest.assignedTo, opt => opt.Ignore());

            CreateMap<TicketComment, TicketCommentInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.CommentText))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")));

            CreateMap<User, UserInfo>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.FullName));

        }
    }
}

