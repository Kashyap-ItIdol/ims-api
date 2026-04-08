using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        async Task<User> IUserRepository.GetByEmailAsync(string email)
        {
            var result =  await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);

            return result ?? throw new Exception("User not found.");
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await ((IUserRepository)this).GetByEmailAsync(email);
        }

        async Task<bool>IUserRepository.CheckUserExixst(string email)
        {
            var result = await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);

            return result == null ? false : true;
        }

        public async Task<bool> CheckUserExixst(string email)
        {
            return await ((IUserRepository)this).CheckUserExixst(email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
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
