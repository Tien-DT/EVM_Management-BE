using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(AppDbContext context) : base(context)
        {
        }

       
        public override async Task<UserProfile?> GetByIdAsync(Guid id)
        {
           
            return await _dbSet
                .Where(u => u.Id == id)
                .Select(u => new UserProfile
                {
                    Id = u.Id,
                    AccountId = u.AccountId,
                    DealerId = u.DealerId,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    CardId = u.CardId,
                    CreatedDate = u.CreatedDate,
                    ModifiedDate = u.ModifiedDate,
                    DeletedDate = u.DeletedDate,
                    IsDeleted = u.IsDeleted,
                    Account = new Account { Id = u.Account.Id, Role = u.Account.Role, IsActive = u.Account.IsActive },
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Account)
                .Select(u => new UserProfile
                {
                    Id = u.Id,
                    AccountId = u.AccountId,
                    DealerId = u.DealerId,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    CardId = u.CardId,
                    CreatedDate = u.CreatedDate,
                    ModifiedDate = u.ModifiedDate,
                    DeletedDate = u.DeletedDate,
                    IsDeleted = u.IsDeleted,
                    Account = new Account { Id = u.Account.Id, Role = u.Account.Role, IsActive = u.Account.IsActive },
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .ToListAsync();
        }

        public async Task<System.Collections.Generic.IEnumerable<UserProfile>> GetByRoleAndStatusAsync(EVMManagement.DAL.Models.Enums.AccountRole role, bool? isActive)
        {
            var query = _dbSet
                .Include(u => u.Account)
                .Where(u => u.Account.Role == role);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.Account.IsActive == isActive.Value);
            }

            return await query
                .Select(u => new UserProfile
                {
                    Id = u.Id,
                    AccountId = u.AccountId,
                    DealerId = u.DealerId,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    CardId = u.CardId,
                    CreatedDate = u.CreatedDate,
                    ModifiedDate = u.ModifiedDate,
                    DeletedDate = u.DeletedDate,
                    IsDeleted = u.IsDeleted,
                    Account = new Account { Id = u.Account.Id, Role = u.Account.Role, IsActive = u.Account.IsActive },
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .ToListAsync();
        }

        public async Task<System.Collections.Generic.IEnumerable<UserProfile>> GetByDealerIdAsync(Guid dealerId)
        {
            return await _dbSet
                .Include(u => u.Account)
                .Where(u => u.DealerId == dealerId)
                .Select(u => new UserProfile
                {
                    Id = u.Id,
                    AccountId = u.AccountId,
                    DealerId = u.DealerId,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    CardId = u.CardId,
                    CreatedDate = u.CreatedDate,
                    ModifiedDate = u.ModifiedDate,
                    DeletedDate = u.DeletedDate,
                    IsDeleted = u.IsDeleted,
                    Account = new Account { Id = u.Account.Id, Role = u.Account.Role, IsActive = u.Account.IsActive },
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .ToListAsync();
        }
    }
}
