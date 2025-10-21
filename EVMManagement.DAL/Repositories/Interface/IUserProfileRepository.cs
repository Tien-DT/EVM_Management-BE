using System;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IUserProfileRepository : IGenericRepository<UserProfile>
    {
        IQueryable<UserProfile> GetByRoleAndStatusAsync(AccountRole role, bool? isActive);
        IQueryable<UserProfile> GetByDealerIdAsync(Guid dealerId);
        Task<UserProfile?> GetManagerByDealerIdAsync(Guid dealerId);
        Task<UserProfile?> GetByAccountIdAsync(Guid accountId);
        IQueryable<UserProfile> GetAllAsync();
        Task<UserProfile?> GetByIdAsync(Guid id);
    }
}
