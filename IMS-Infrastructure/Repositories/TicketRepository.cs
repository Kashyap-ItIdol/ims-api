using IMS_Application.Interfaces;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        //private readonly AppDbContext _context;

        //public TicketRepository(AppDbContext context)
        //{
        //    _context = context;
        //}

        //public async Task<List<int>> GetRecentUserIdsAsync(int count)
        //{
        //    return await _context.Tickets
        //        .OrderByDescending(t => t.CreatedAt)
        //        .Select(t => t.CreatedBy)
        //        .Distinct()
        //        .Take(count)
        //        .ToListAsync();
        //}
    }
}