using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IUserProfileService
    {
        Task<IEnumerable<UserProfile>> GetAllAsync();
        Task<UserProfile?> GetByIdAsync(Guid id);
        Task<UserProfile> CreateAsync(UserProfile entity);
        Task<UserProfile?> UpdateAsync(Guid id, UserProfile entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
