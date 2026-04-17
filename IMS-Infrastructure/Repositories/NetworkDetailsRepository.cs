using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;

namespace IMS_Infrastructure.Repositories
{
    public class NetworkDetailsRepository : INetworkDetailsRepository
    {
        private readonly AppDbContext _context;

        public NetworkDetailsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NetworkDetail?> GetByAssetIdAsync(int assetId)
        {
            return await _context.Set<NetworkDetail>()
                .FirstOrDefaultAsync(x => x.AssetId == assetId);
        }
    }
}
