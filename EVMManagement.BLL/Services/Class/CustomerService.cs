using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Customer> CreateCustomerAsync(CustomerCreateDto dto)
        {
            var customer = new Customer
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                Gender = dto.Gender,
                Address = dto.Address,
                Dob = dto.Dob,
                CardId = dto.CardId
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return customer;
        }

        public async Task<PagedResult<CustomerResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Customers.GetQueryable();
            var totalCount = await _unitOfWork.Customers.CountAsync();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CustomerResponse
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    Email = x.Email,
                    Gender = x.Gender,
                    Address = x.Address,
                    Dob = x.Dob,
                    CardId = x.CardId,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<CustomerResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<CustomerResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return null;

            return new CustomerResponse
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Phone = entity.Phone,
                Email = entity.Email,
                Gender = entity.Gender,
                Address = entity.Address,
                Dob = entity.Dob,
                CardId = entity.CardId,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<CustomerResponse?> UpdateAsync(Guid id, CustomerUpdateDto dto)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.FullName != null) entity.FullName = dto.FullName;
            if (dto.Phone != null) entity.Phone = dto.Phone;
            if (dto.Email != null) entity.Email = dto.Email;
            if (dto.Gender != null) entity.Gender = dto.Gender;
            if (dto.Address != null) entity.Address = dto.Address;
            if (dto.Dob.HasValue) entity.Dob = dto.Dob;
            if (dto.CardId != null) entity.CardId = dto.CardId;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Customers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<CustomerResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Customers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Customers.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
