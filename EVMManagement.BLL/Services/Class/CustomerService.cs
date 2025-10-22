using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                Dob = DateTimeHelper.ToUtc(dto.Dob),
                CardId = dto.CardId
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return customer;
        }

        public async Task<PagedResult<CustomerResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Customers.GetQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CustomerResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<CustomerResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<CustomerResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Customers.GetQueryable()
                .Where(x => x.Orders.Any(o => o.DealerId == dealerId));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CustomerResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<CustomerResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<CustomerResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<CustomerResponse>(entity);
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
            if (dto.Dob.HasValue) entity.Dob = DateTimeHelper.ToUtc(dto.Dob);
            if (dto.CardId != null) entity.CardId = dto.CardId;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Customers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerResponse>(entity);
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

            return _mapper.Map<CustomerResponse>(entity);
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
