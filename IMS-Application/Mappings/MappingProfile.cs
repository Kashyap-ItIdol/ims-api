using AutoMapper;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, GetCategoryDto>();
            CreateMap<SubCategory, SubCategoryDto>();
            CreateMap<Category, ListCategoriesDto>();
            CreateMap<User, UserInfo>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.FullName));
            CreateMap<User, UserInfoDto>()
             .ForMember(dest => dest.RoleId,
                        opt => opt.MapFrom(src => src.RoleId));

            CreateMap<CreateTicketRequestDto, Ticket>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TicketType, opt => opt.Ignore())
                .ForMember(dest => dest.TicketPriority, opt => opt.MapFrom(src => (TicketPriority)Enum.Parse(typeof(TicketPriority), src.Priority)))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.SubCategoryId, opt => opt.MapFrom(src => src.SubCategoryId));

            CreateMap<Ticket, TicketInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => $"TKT-{src.Id}"))
                 .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TicketType, opt => opt.MapFrom(src => src.TicketType.ToString()))
                .ForMember(dest => dest.TicketPriority, opt => opt.MapFrom(src => src.TicketPriority.ToString()))
                .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.updatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.assetId, opt => opt.MapFrom(src => src.AssetId.HasValue ? src.AssetId.Value.ToString() : "null"))
                .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src => src.CategoryId.HasValue ? src.CategoryId.Value.ToString() : "null"))
                .ForMember(dest => dest.subCategoryId, opt => opt.MapFrom(src => src.SubCategoryId.HasValue ? src.SubCategoryId.Value.ToString() : "null"))
                .ForMember(dest => dest.createdBy, opt => opt.Ignore())
                .ForMember(dest => dest.assignedTo, opt => opt.Ignore());

            CreateMap<TicketComment, TicketCommentResponseDto>()
                .ForMember(dest => dest.ticketId, opt => opt.MapFrom(src => src.TicketId.ToString()))
                .ForMember(dest => dest.text, opt => opt.MapFrom(src => src.CommentText))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.updatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null));

            CreateMap<TicketComment, TicketCommentInfo>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Likes.Count));

            CreateMap<TicketCommentLike, CommentLikeResponseDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")));

            CreateMap<TicketCommentReaction, CommentReactionResponseDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")));

            CreateMap<Ticket, UpdateTicketStatusResponseDto>()
                .ForMember(dest => dest.updatedStatus, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.updatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        }
    }
}

