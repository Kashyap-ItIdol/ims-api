using System;
using System.Collections.Generic;
using System.Text;
using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface IAssetRepository
    {
        Task AddRangeAsync(List<Asset> assets);
        Task<bool> SerialExistsAsync(string serialNo);
        Task SaveChangesAsync();
    }
}
