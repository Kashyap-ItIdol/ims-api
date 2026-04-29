using AutoMapper;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();

            CreateMap<CreateChildAssetDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAssetId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
                .ForMember(dest => dest.AssignDate, opt => opt.Ignore())
                .ForMember(dest => dest.ExpectedReturnDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsClient, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAsset, opt => opt.Ignore())
                .ForMember(dest => dest.ChildAssets, opt => opt.Ignore());

            CreateMap<AssetItemDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAssetId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
                .ForMember(dest => dest.AssignDate, opt => opt.Ignore())
                .ForMember(dest => dest.ExpectedReturnDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsClient, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAsset, opt => opt.Ignore())
                .ForMember(dest => dest.ChildAssets, opt => opt.Ignore());

            CreateMap<UpdateAssetDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAssetId, opt => opt.Ignore())
                .ForMember(dest => dest.ChildAssets, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            CreateMap<Asset, AssetResponseDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.Location : null))
                .ForMember(dest => dest.TableNo, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.TableNo : null))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ChildAssets));

            CreateMap<Asset, AssetListDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.AssetStatus.Status))
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : null));

            CreateMap<Asset, AssetOverviewDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.AssetStatus.Status))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.AssetCondition.Condition))
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            CreateMap<Asset, ChildAssetDto>();

            CreateMap<Asset, AssetAssignmentDto>()
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedUser.FullName))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.AssignedUser.Department.Name))
                .ForMember(dest => dest.OfficeNo, opt => opt.MapFrom(src => src.AssignedUser.Location))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.AssignedTo))
                .ForMember(dest => dest.History, opt => opt.Ignore())
                .ForMember(dest => dest.Network, opt => opt.Ignore());

            CreateMap<NetworkDetail, NetworkDetailsDto>();
            CreateMap<NetworkDetailsDto, NetworkDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore());

            CreateMap<AssetHistory, AssetHistoryDto>()
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.ItemName));

            CreateMap<Asset, GetAssetByIdResponseDto>()
                .ForMember(dest => dest.IsParent, opt => opt.MapFrom(src => src.ParentAssetId == null))
                .ForMember(dest => dest.Overview, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Assignment, opt => opt.MapFrom(src => src));

            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
                .ForMember(dest => dest.Department,
                     opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

            CreateMap<CreateUserDto, User>()
                   .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                   .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                   .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                   .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                   .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<User, UserInfoDto>();
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

