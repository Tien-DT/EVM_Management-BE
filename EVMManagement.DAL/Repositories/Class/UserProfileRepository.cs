using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Class
{
    public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<UserProfile> GetByRoleAndStatusAsync(AccountRole role, bool? isActive)
        {
            var query = _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer)
                .Where(u => u.Account.Role == role);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.Account.IsActive == isActive.Value);
            }

            return query;
        }

        public IQueryable<UserProfile> GetByDealerIdAsync(Guid dealerId)
        {
            return _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer)
                .Where(u => u.DealerId == dealerId);
        }

        public async Task<UserProfile?> GetManagerByDealerIdAsync(Guid dealerId)
        {
            return await _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer)
                .Where(u => u.DealerId == dealerId && u.Account.Role == AccountRole.DEALER_MANAGER && u.Account.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<UserProfile?> GetByAccountIdAsync(Guid accountId)
        {
            return await _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer)
                .Where(u => u.AccountId == accountId)
                .FirstOrDefaultAsync();
        }

        public IQueryable<UserProfile> GetAllAsync()
        {
            return _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer);
        }

        public async Task<UserProfile?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Account)
                .Include(u => u.Dealer)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}