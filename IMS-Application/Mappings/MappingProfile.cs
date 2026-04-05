using AutoMapper;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.Role,
                       opt => opt.MapFrom(src => src.Role.Name));
        }
    }
}
