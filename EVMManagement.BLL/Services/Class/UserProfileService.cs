using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;

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

        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            return await _userProfileRepository.GetAllAsync();
        }

        public async Task<UserProfile?> GetByIdAsync(Guid id)
        {
            return await _userProfileRepository.GetByIdAsync(id);
        }

        public async Task<UserProfile> CreateAsync(UserProfile entity)
        {
            await _userProfileRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<UserProfile?> UpdateAsync(Guid id, UserProfile entity)
        {
            var existing = await _userProfileRepository.GetByIdAsync(id);
            if (existing == null) return null;

            // AccountId must not be changed via update
            // existing.AccountId = entity.AccountId; // intentionally not changed
            existing.DealerId = entity.DealerId;
            existing.FullName = entity.FullName;
            existing.Phone = entity.Phone;
            existing.CardId = entity.CardId;

            _userProfileRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _userProfileRepository.GetByIdAsync(id);
            if (existing == null) return false;
            _userProfileRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
