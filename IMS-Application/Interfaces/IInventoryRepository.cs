using IMS_Application.DTOs;

namespace IMS_Application.Interfaces
{
    public interface IInventoryRepository
    {
        Task<int> CreateInventoryAsync(InventoryCreateDto dto);
    }
}