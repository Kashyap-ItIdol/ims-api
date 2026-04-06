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
            CreateMap<InventoryCreateDto, Inventory>();
            CreateMap<PurchaseDetailDto, PurchaseDetail>();
            CreateMap<AccessoryDto, Accessory>();
            CreateMap<InventoryAssignmentDto, InventoryAssignment>();
        }
    }
}
