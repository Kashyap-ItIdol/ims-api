using AutoMapper;
using IMS_Application.DTOs;
using IMS_Application.DTOs.SubCategory;
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
            CreateMap<SubCategory, DTOs.SubCategory.SubCategoryDto>();

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


            CreateMap<CreateAssetDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetName.Trim()))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.Trim()))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Trim()))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber.Trim()))
                .ForMember(dest => dest.ClientPOC, opt => opt.MapFrom(src => src.ClientPOC.Trim()))
                .ForMember(dest => dest.SalesPOC, opt => opt.MapFrom(src => src.SalesPOC.Trim()));

            CreateMap<Asset, AssetResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory != null ? src.SubCategory.Name : "Unknown"));

            CreateMap<Asset, CreateAssetDto>()
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber))
                .ForMember(dest => dest.ClientPOC, opt => opt.MapFrom(src => src.ClientPOC))
                .ForMember(dest => dest.SalesPOC, opt => opt.MapFrom(src => src.SalesPOC));

            CreateMap<CreateCategoryRequestDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()));


            CreateMap<CreateSubCategoryDto, SubCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()));

            CreateMap<SubCategory, DTOs.SubCategory.SubCategoryDto>();

            CreateMap<CreateClientAssetDto, ClientAsset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetName.Trim()))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.Trim()))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Trim()))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber.Trim()))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.Trim()))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ClientName.Trim()))
                .ForMember(dest => dest.ClientPOC, opt => opt.MapFrom(src => src.ClientPOC.Trim()))
                .ForMember(dest => dest.SalesPOC, opt => opt.MapFrom(src => src.SalesPOC.Trim()))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Trim()));

            CreateMap<EditClientAssetFullDto, ClientAsset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetName.Trim()))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.Trim()))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Trim()))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber.Trim()))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.Trim()))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ClientName.Trim()))
                .ForMember(dest => dest.ClientPOC, opt => opt.MapFrom(src => src.ClientPOC.Trim()))
                .ForMember(dest => dest.SalesPOC, opt => opt.MapFrom(src => src.SalesPOC.Trim()))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Trim()));

            CreateMap<EditClientAssetQuickDto, ClientAsset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.AssetName.Trim()))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber.Trim()));

            // AssetAssignment Mappings
            CreateMap<AssetAssignmentDto, AssetAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<CreateAndAssignAssetDto, Asset>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Assigned"))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.ItemName.Trim()))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.Trim()))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Trim()))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.SerialNumber) ? "N/A" : src.SerialNumber.Trim()))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.Trim()))
                .ForMember(dest => dest.ClientPOC, opt => opt.MapFrom(src => src.ClientPOC.Trim()))
                .ForMember(dest => dest.SalesPOC, opt => opt.MapFrom(src => src.ClientPOC.Trim()));

            CreateMap<CreateAndAssignAssetDto, AssetAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<AssetAssignment, AssetAssignmentResponseDto>()
                .ForMember(dest => dest.IsReturned, opt => opt.MapFrom(src => src.ActualReturnDate.HasValue));

            // AssetAttachment Mappings
            CreateMap<UploadAttachmentDto, ClientAssetAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<ClientAssetAttachment, AttachmentResponseDto>();
        }
    }
}

