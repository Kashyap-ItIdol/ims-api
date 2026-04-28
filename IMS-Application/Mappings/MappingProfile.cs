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
        }
    }
}
