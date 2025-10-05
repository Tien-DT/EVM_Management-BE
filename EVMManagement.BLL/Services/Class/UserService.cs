using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user?.IsDeleted == false ? user : null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetByUsernameAsync(username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.FindAsync(u => !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _unitOfWork.Users.GetActiveUsersAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Business logic validation
            if (await UserExistsAsync(user.Username, user.Email))
            {
                throw new InvalidOperationException("User with this username or email already exists.");
            }

            user.CreatedDate = DateTime.UtcNow;
            user.IsDeleted = false;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
            if (existingUser == null || existingUser.IsDeleted)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            // Soft delete
            user.IsDeleted = true;
            user.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _unitOfWork.Users.AnyAsync(u => 
                (u.Username == username || u.Email == email) && !u.IsDeleted);
        }
    }
}
