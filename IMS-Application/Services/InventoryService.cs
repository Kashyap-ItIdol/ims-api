using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;

namespace IMS_Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<int> CreateInventoryAsync(InventoryCreateDto dto)
        {
            return await _inventoryRepository.CreateInventoryAsync(dto);
        }
    }
}