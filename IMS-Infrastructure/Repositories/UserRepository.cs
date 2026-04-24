using IMS_Application.Interfaces;
using IMS_Domain.Constants;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .Include(u => u.RefreshTokens)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(rt => rt.Token == refreshToken));
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> TableAlreadyAssignedAsync(string tableNo)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u =>
                    u.TableNo == tableNo &&
                    u.IsActive &&
                    !u.IsDeleted);
        }

        public async Task<List<User>> GetAllWithRolesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Department)
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<User>> SearchAsync(string query)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted &&
                    (EF.Functions.Like(u.FullName, $"%{query}%") ||
                     EF.Functions.Like(u.Email, $"%{query}%")))
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersWithOpenTicketsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(u =>
                    _context.Tickets.Any(t =>
                        t.CreatedBy == u.Id &&
                        t.Status == Status.Open)
                    && u.IsActive && !u.IsDeleted)
                .ToListAsync();
        }
    }
}