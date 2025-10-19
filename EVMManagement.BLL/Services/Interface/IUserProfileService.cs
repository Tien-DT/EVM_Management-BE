using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IUserProfileService
    {
        Task<PagedResult<UserProfileResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<UserProfileResponse>> GetByRoleAndStatusAsync(AccountRole role, bool? isActive, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<UserProfileResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<UserProfileResponse?> GetByAccountIdAsync(Guid accId);
        Task<UserProfileResponse?> GetByIdAsync(Guid id);
    Task<UserProfileResponse?> GetManagerByDealerIdAsync(Guid dealerId);
        Task<UserProfileResponse?> UpdateAsync(Guid accId, UserProfile entity);
        Task<UserProfileResponse?> UpdateIsDeletedAsync(Guid accId, bool isDeleted);
       
    }
}
