using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.User;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IUserProfileService
    {
        Task<PagedResult<UserProfileResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<UserProfileResponse>> GetByRoleAndStatusAsync(EVMManagement.DAL.Models.Enums.AccountRole role, bool? isActive, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<UserProfileResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<UserProfileResponse?> GetByAccountIdAsync(Guid accId);
        Task<UserProfileResponse?> GetByIdAsync(Guid id);
        Task<UserProfileResponse> CreateAsync(EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<UserProfileResponse?> UpdateAsync(Guid id, EVMManagement.DAL.Models.Entities.UserProfile entity);
        Task<UserProfileResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
