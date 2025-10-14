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
        Task<IEnumerable<UserProfileResponse>> GetByDealerIdAsync(Guid dealerId);
        Task<UserProfileResponse?> GetByAccountIdAsync(Guid accId);
        Task<UserProfileResponse?> GetByIdAsync(Guid id);
        Task<UserProfileResponse> CreateAsync(EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<UserProfileResponse?> UpdateAsync(Guid id, EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<UserProfileResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
