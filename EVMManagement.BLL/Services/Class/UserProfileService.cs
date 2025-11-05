using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.User;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Class
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<UserProfileResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.UserProfiles.GetAllAsync();

            var totalCount = await query.CountAsync(x => !x.IsDeleted);

            var items = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToResponse).ToList();

            return PagedResult<UserProfileResponse>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<UserProfileResponse?> GetByAccountIdAsync(Guid accId)
        {
            var entity = await _unitOfWork.UserProfiles.GetByAccountIdAsync(accId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<UserProfileResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.UserProfiles.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<UserProfileResponse?> GetManagerByDealerIdAsync(Guid dealerId)
        {
            var managerEntity = await _unitOfWork.UserProfiles.GetManagerByDealerIdAsync(dealerId);

           
            if (managerEntity == null)
            {
                managerEntity = await _unitOfWork.UserProfiles.GetByDealerIdAsync(dealerId)
                    .Where(u => u.Account != null && u.Account.Role == EVMManagement.DAL.Models.Enums.AccountRole.DEALER_MANAGER)
                    .OrderByDescending(u => u.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            return managerEntity == null ? null : MapToResponse(managerEntity);
        }

        public async Task<PagedResult<UserProfileResponse>> GetByRoleAndStatusAsync(AccountRole role, bool? isActive, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.UserProfiles.GetByRoleAndStatusAsync(role, isActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToResponse).ToList();

            return PagedResult<UserProfileResponse>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<UserProfileResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.UserProfiles.GetByDealerIdAsync(dealerId);

            var totalCount = await query.CountAsync(x => !x.IsDeleted);

            var items = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToResponse).ToList();

            return PagedResult<UserProfileResponse>.Create(responses, totalCount, pageNumber, pageSize);
        }

       

        public async Task<UserProfileResponse?> UpdateAsync(Guid accId, UserProfile entity, string? email = null)
        {
            var existing = await _unitOfWork.UserProfiles.GetByAccountIdAsync(accId);
            if (existing == null) return null;

            if (entity.DealerId.HasValue)
            {
                if (entity.DealerId == Guid.Empty)
                    throw new ArgumentException("DealerId is not valid.", nameof(entity.DealerId));

                existing.DealerId = entity.DealerId;
            }

            existing.FullName = entity.FullName;

            if (entity.Phone != null)
            {
                if (entity.Phone == string.Empty)
                {
                    existing.Phone = null;
                }
                else
                {
                    if (entity.Phone.Length > 20)
                        throw new ArgumentException("Phone must be at most 20 characters.", nameof(entity.Phone));
                    bool phoneExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => u.Phone == entity.Phone && u.Id != existing.Id);
                    if (phoneExists)
                        throw new ArgumentException("Phone is already in use by another user.", nameof(entity.Phone));
                    existing.Phone = entity.Phone;
                }
            }

            
            if (entity.CardId != null)
            {
                if (entity.CardId == string.Empty)
                {
                    existing.CardId = null;
                }
                else
                {
                    if (entity.CardId.Length > 50)
                        throw new ArgumentException("CardId must be at most 12 characters.", nameof(entity.CardId));
                    bool cardExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => u.CardId == entity.CardId && u.Id != existing.Id);
                    if (cardExists)
                        throw new ArgumentException("CardId is already in use by another user.", nameof(entity.CardId));
                    existing.CardId = entity.CardId;
                }
            }

            // Update email in Account entity if provided
            if (!string.IsNullOrWhiteSpace(email))
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(accId);
                if (account == null)
                    throw new ArgumentException("Account not found.", nameof(accId));

                // Check if email is already in use by another account
                var emailExists = await _unitOfWork.Accounts.AnyAsync(a => a.Email == email && a.Id != accId && !a.IsDeleted);
                
                if (emailExists)
                    throw new ArgumentException("Email is already in use by another account.", nameof(email));

                account.Email = email;
                _unitOfWork.Accounts.Update(account);
            }

            _unitOfWork.UserProfiles.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing);
        }

        public async Task<UserProfileResponse?> UpdateByUserProfileIdAsync(Guid userProfileId, UserProfile entity, string? email = null)
        {
            // Get UserProfile by ID
            var existing = await _unitOfWork.UserProfiles.GetByIdAsync(userProfileId);
            if (existing == null) return null;

            // Get the Account ID from the UserProfile
            var accountId = existing.AccountId;

            if (entity.DealerId.HasValue)
            {
                if (entity.DealerId == Guid.Empty)
                    throw new ArgumentException("DealerId is not valid.", nameof(entity.DealerId));

                existing.DealerId = entity.DealerId;
            }

            existing.FullName = entity.FullName;

            if (entity.Phone != null)
            {
                if (entity.Phone == string.Empty)
                {
                    existing.Phone = null;
                }
                else
                {
                    if (entity.Phone.Length > 20)
                        throw new ArgumentException("Phone must be at most 20 characters.", nameof(entity.Phone));
                    bool phoneExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => u.Phone == entity.Phone && u.Id != existing.Id);
                    if (phoneExists)
                        throw new ArgumentException("Phone is already in use by another user.", nameof(entity.Phone));
                    existing.Phone = entity.Phone;
                }
            }

            
            if (entity.CardId != null)
            {
                if (entity.CardId == string.Empty)
                {
                    existing.CardId = null;
                }
                else
                {
                    if (entity.CardId.Length > 50)
                        throw new ArgumentException("CardId must be at most 12 characters.", nameof(entity.CardId));
                    bool cardExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => u.CardId == entity.CardId && u.Id != existing.Id);
                    if (cardExists)
                        throw new ArgumentException("CardId is already in use by another user.", nameof(entity.CardId));
                    existing.CardId = entity.CardId;
                }
            }

            // Update email in Account entity if provided
            if (!string.IsNullOrWhiteSpace(email))
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                    throw new ArgumentException("Account not found.", nameof(accountId));

                // Check if email is already in use by another account
                var emailExists = await _unitOfWork.Accounts.AnyAsync(a => a.Email == email && a.Id != accountId && !a.IsDeleted);
                
                if (emailExists)
                    throw new ArgumentException("Email is already in use by another account.", nameof(email));

                account.Email = email;
                _unitOfWork.Accounts.Update(account);
            }

            _unitOfWork.UserProfiles.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing);
        }

        public async Task<UserProfileResponse?> UpdateIsDeletedAsync(Guid accId, bool isDeleted)
        {
            var existing = await _unitOfWork.UserProfiles.GetByAccountIdAsync(accId);
            if (existing == null) return null;

            existing.IsDeleted = isDeleted;
            existing.DeletedDate = isDeleted ? DateTime.UtcNow : null;
            _unitOfWork.UserProfiles.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing);
        }


        private static UserProfileResponse MapToResponse(UserProfile u)
        {
            return new UserProfileResponse
            {
                Id = u.Id,
                AccountId = u.AccountId,
                DealerId = u.DealerId,
                FullName = u.FullName,
                Phone = u.Phone,
                CardId = u.CardId,
                Dealer = u.DealerId == null ? null : new DealerDto { Id = u.Dealer!.Id, Name = u.Dealer!.Name },
                Account = u.Account == null ? null : new AccountDto { Role = u.Account.Role, IsActive = u.Account.IsActive, Email = u.Account.Email },
                CreatedDate = u.CreatedDate,
                ModifiedDate = u.ModifiedDate,
                DeletedDate = u.DeletedDate,
                IsDeleted = u.IsDeleted
            };
        }
    }
}