using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<int> CreateInventoryAsync(InventoryCreateDto dto);
    }
}