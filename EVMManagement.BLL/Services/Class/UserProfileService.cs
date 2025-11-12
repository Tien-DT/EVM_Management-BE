using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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

            // Track what fields are being updated
            bool hasChanges = false;

            if (entity.DealerId.HasValue)
            {
                if (entity.DealerId == Guid.Empty)
                    throw new ArgumentException("DealerId không hợp lệ.", nameof(entity.DealerId));

                if (existing.DealerId != entity.DealerId)
                {
                    existing.DealerId = entity.DealerId;
                    hasChanges = true;
                }
            }

            if (entity.FullName != null)
            {
                var newFullName = entity.FullName.Trim();
                if (newFullName.Length == 0)
                    throw new ArgumentException("Ho ten khong duoc de trong.", nameof(entity.FullName));

                if (existing.FullName != newFullName)
                {
                    existing.FullName = newFullName;
                    hasChanges = true;
                }
            }

            if (entity.Phone != null)
            {
                var phoneValue = entity.Phone.Trim();
                if (phoneValue == string.Empty)
                {
                    if (existing.Phone != null)
                    {
                        existing.Phone = null;
                        hasChanges = true;
                    }
                }
                else
                {
                    // Normalize existing phone for comparison
                    var existingPhone = existing.Phone?.Trim();
                    
                    // Skip update if Phone hasn't changed
                    if (!string.Equals(existingPhone, phoneValue, StringComparison.Ordinal))
                    {
                        if (!Regex.IsMatch(phoneValue, @"^\d{10}$"))
                            throw new ArgumentException("Số điện thoại phải gồm đúng 10 chữ số.", nameof(entity.Phone));
                        
                        bool phoneExists = await _unitOfWork.UserProfiles
                            .GetQueryable()
                            .AsNoTracking()
                            .AnyAsync(u => u.Phone == phoneValue && u.Id != existing.Id);
                        
                        if (phoneExists)
                            throw new ArgumentException("Số điện thoại đã được sử dụng.");
                        
                        existing.Phone = phoneValue;
                        hasChanges = true;
                    }
                }
            }

            if (entity.CardId != null)
            {
                var cardValue = entity.CardId.Trim();
                if (cardValue == string.Empty)
                {
                    if (existing.CardId != null)
                    {
                        existing.CardId = null;
                        hasChanges = true;
                    }
                }
                else
                {
                    // Normalize existing CardId for comparison
                    var existingCardId = existing.CardId?.Trim();
                    
                    // Skip update if CardId hasn't changed (exact match)
                    if (!string.Equals(existingCardId, cardValue, StringComparison.Ordinal))
                    {
                        if (!Regex.IsMatch(cardValue, @"^\d{12}$"))
                            throw new ArgumentException("Căn cước phải gồm đúng 12 chữ số.", nameof(entity.CardId));
                        
                        // Check if this CardId is already used by another user
                        bool cardExists = await _unitOfWork.UserProfiles
                            .GetQueryable()
                            .AsNoTracking()
                            .AnyAsync(u => u.CardId == cardValue && u.Id != existing.Id);
                        
                        if (cardExists)
                            throw new ArgumentException("Căn cước đã được sử dụng.");
                        
                        existing.CardId = cardValue;
                        hasChanges = true;
                    }
                }
            }

            // Update email in Account entity if provided
            if (!string.IsNullOrWhiteSpace(email))
            {
                var trimmedEmail = email.Trim();
                var account = await _unitOfWork.Accounts.GetByIdAsync(accId);
                if (account == null)
                    throw new ArgumentException("Không tìm thấy tài khoản.", nameof(accId));

                if (account.Email != trimmedEmail)
                {
                    // Check if email is already in use by another account
                    var emailExists = await _unitOfWork.Accounts
                        .GetQueryable()
                        .AsNoTracking()
                        .AnyAsync(a => a.Email == trimmedEmail && a.Id != accId);
                    
                    if (emailExists)
                        throw new ArgumentException("Email đã được sử dụng bởi tài khoản khác.");

                    account.Email = trimmedEmail;
                    _unitOfWork.Accounts.Update(account);
                    hasChanges = true;
                }
            }

            // Only save if there are actual changes
            if (hasChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            
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
                Account = u.Account == null ? null : new AccountDto
                {
                    Role = u.Account.Role,
                    IsActive = u.Account.IsActive,
                    Email = u.Account.Email,
                    IsPasswordChange = u.Account.IsPasswordChange
                },
                CreatedDate = u.CreatedDate,
                ModifiedDate = u.ModifiedDate,
                DeletedDate = u.DeletedDate,
                IsDeleted = u.IsDeleted
            };
        }
    }
}
