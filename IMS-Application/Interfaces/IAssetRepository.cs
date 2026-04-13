using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Interfaces
{
    public interface IAssetRepository
    {
        Task AddRangeAsync(List<Asset> assets);
        Task<bool> SerialExistsAsync(string serialNo);
        // Task SaveChangesAsync();
        Task<List<Asset>> GetAllAsync();

        // delete asset
        Task<bool> HasChildrenAsync(int id);

        // update asset

        Task<Asset?> GetByIdAsync(int id);
        void SoftDelete(Asset asset);

      
    }

}

