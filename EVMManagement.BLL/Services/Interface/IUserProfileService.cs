using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Response.User;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IUserProfileService
    {
        Task<IEnumerable<UserProfileResponse>> GetAllAsync();
        Task<IEnumerable<UserProfileResponse>> GetByRoleAndStatusAsync(EVMManagement.DAL.Models.Enums.AccountRole role, bool? isActive);
        Task<UserProfileResponse?> GetByIdAsync(Guid id);
        Task<UserProfileResponse> CreateAsync(EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<UserProfileResponse?> UpdateAsync(Guid id, EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
