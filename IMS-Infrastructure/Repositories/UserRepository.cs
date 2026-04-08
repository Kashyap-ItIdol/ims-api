using IMS_Application.Interfaces;
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
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _dbSet.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .Include(u => u.RefreshTokens)
                .Include(u => u.Role) //  need the Role to generate the new JWT claims!
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
    }
}
