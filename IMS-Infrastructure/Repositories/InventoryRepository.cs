using AutoMapper;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Domain.Constants;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;

namespace IMS_Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public InventoryRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> CreateInventoryAsync(InventoryCreateDto dto)
        {
            // 🔥 Map Inventory
            var inventory = _mapper.Map<Inventory>(dto);

            // ENUM FIX
            inventory.Status = (AssetStatus)dto.Status;
            inventory.Condition = (ConditionType)dto.Condition;
            inventory.ImageUrl = dto.Image;

            // 🔥 Purchase
            if (dto.PurchaseDetail != null)
            {
                inventory.PurchaseDetail = _mapper.Map<PurchaseDetail>(dto.PurchaseDetail);
            }

            // 🔥 Accessories
            if (dto.Accessories != null && dto.Accessories.Any())
            {
                inventory.Accessories = dto.Accessories
                    .Select(a =>
                    {
                        var acc = _mapper.Map<Accessory>(a);
                        acc.Condition = (ConditionType)a.Condition;
                        return acc;
                    }).ToList();
            }

            // 🔥 Assignment
            if (dto.Assignment != null)
            {
                inventory.Assignments = new List<InventoryAssignment>
                {
                    _mapper.Map<InventoryAssignment>(dto.Assignment)
                };
            }

            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();

            return inventory.Id;
        }
    }
}