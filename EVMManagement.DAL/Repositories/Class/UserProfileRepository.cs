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
                    // Keep Account as null to avoid loading unnecessary data
                    Account = null!,
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .FirstOrDefaultAsync();
        }

        public override async Task<System.Collections.Generic.IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _dbSet
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
                    Account = null!,
                    Dealer = u.DealerId == null ? null : new Dealer { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                })
                .ToListAsync();
        }
    }
}
