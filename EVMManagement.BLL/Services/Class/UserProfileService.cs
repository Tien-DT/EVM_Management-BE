using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.BLL.DTOs.Response.User;

namespace EVMManagement.BLL.Services.Class
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileService(IUserProfileRepository userProfileRepository, IUnitOfWork unitOfWork)
        {
            _userProfileRepository = userProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAllAsync()
        {
            var list = await _userProfileRepository.GetAllAsync();
            return list.Select(MapToResponse);
        }

        public async Task<UserProfileResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _userProfileRepository.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<IEnumerable<UserProfileResponse>> GetByRoleAndStatusAsync(EVMManagement.DAL.Models.Enums.AccountRole role, bool? isActive)
        {
            var list = await _userProfileRepository.GetByRoleAndStatusAsync(role, isActive);
            return list.Select(MapToResponse);
        }

        public async Task<IEnumerable<UserProfileResponse>> GetByDealerIdAsync(Guid dealerId)
        {
            var list = await _userProfileRepository.GetByDealerIdAsync(dealerId);
            return list.Select(MapToResponse);
        }

        public async Task<UserProfileResponse> CreateAsync(UserProfile entity)
        {
            await _userProfileRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<UserProfileResponse?> UpdateAsync(Guid id, UserProfile entity)
        {
            var existing = await _userProfileRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.DealerId = entity.DealerId;
            existing.FullName = entity.FullName;
            existing.Phone = entity.Phone;
            existing.CardId = entity.CardId;

            _userProfileRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _userProfileRepository.GetByIdAsync(id);
            if (existing == null) return false;
            _userProfileRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
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
                Dealer = u.DealerId == null ? null : new DealerDto { Id = u.Dealer!.Id, Name = u.Dealer!.Name }
                , Account = u.Account == null ? null : new AccountDto { Role = u.Account.Role, IsActive = u.Account.IsActive }
            };
        }
    }
}
