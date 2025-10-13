using System;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IUserProfileRepository : IGenericRepository<UserProfile>
    {
        Task<System.Collections.Generic.IEnumerable<UserProfile>> GetByRoleAndStatusAsync(AccountRole role, bool? isActive);
        Task<System.Collections.Generic.IEnumerable<UserProfile>> GetByDealerIdAsync(Guid dealerId);
    }
}
