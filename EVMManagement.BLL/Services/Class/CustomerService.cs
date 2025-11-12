using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.Exceptions;
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

        public async Task<Customer> CreateCustomerAsync(CustomerCreateDto dto, Guid? managedByAccountId = null)
        {
            var validationErrors = new List<string>();
            var customersQuery = _unitOfWork.Customers.GetQueryable().Where(c => !c.IsDeleted);

            var normalizedPhone = dto.Phone?.Trim() ?? string.Empty;
            var normalizedEmail = dto.Email?.Trim();
            var normalizedCardId = dto.CardId?.Trim();
            var normalizedFullName = dto.FullName?.Trim();
            var normalizedGender = dto.Gender?.Trim();
            var normalizedAddress = dto.Address?.Trim();

            if (!string.IsNullOrEmpty(normalizedPhone))
            {
                var phoneExists = await customersQuery.AnyAsync(c => c.Phone == normalizedPhone);
                if (phoneExists)
                {
                    validationErrors.Add("Số điện thoại đã được sử dụng.");
                }
            }

            if (!string.IsNullOrEmpty(normalizedEmail))
            {
                var lowerEmail = normalizedEmail.ToLower();
                var emailExists = await customersQuery.AnyAsync(c => c.Email != null && c.Email.ToLower() == lowerEmail);
                if (emailExists)
                {
                    validationErrors.Add("Email đã được sử dụng.");
                }
            }

            if (!string.IsNullOrEmpty(normalizedCardId))
            {
                var cardExists = await customersQuery.AnyAsync(c => c.CardId == normalizedCardId);
                if (cardExists)
                {
                    validationErrors.Add("CCCD đã được sử dụng.");
                }
            }

            if (validationErrors.Any())
            {
                throw new CustomerValidationException(validationErrors);
            }

            var customer = new Customer
            {
                FullName = normalizedFullName,
                Phone = normalizedPhone,
                Email = normalizedEmail,
                Gender = normalizedGender,
                Address = normalizedAddress,
                Dob = DateTimeHelper.ToUtc(dto.Dob),
                CardId = normalizedCardId,
                DealerId = dto.DealerId,
                ManagedBy = managedByAccountId
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
                .Where(x => x.DealerId == dealerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CustomerResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<CustomerResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<CustomerResponse>> GetByManagedByAsync(Guid accountId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Customers.GetQueryable()
                .Where(x => x.ManagedBy == accountId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CustomerResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<CustomerResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<CustomerSalesSummaryResponse> GetSalesSummaryByManagedAccountAsync(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var customersQuery = _unitOfWork.Customers.GetQueryable()
                .Where(x => x.ManagedBy == accountId);

            var totalCustomers = await customersQuery.CountAsync();

            var ordersQuery = _unitOfWork.Orders.GetQueryable()
                .Where(o => o.Customer != null && o.Customer.ManagedBy == accountId);

            var fromUtc = DateTimeHelper.ToUtc(fromDate);
            var toUtc = DateTimeHelper.ToUtc(toDate);

            if (fromUtc.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate <= toUtc.Value);
            }

            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = totalOrders == 0
                ? 0m
                : await ordersQuery.SumAsync(o => o.FinalAmount ?? 0m);

            return new CustomerSalesSummaryResponse
            {
                ManagedBy = accountId,
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                FromDate = fromUtc,
                ToDate = toUtc
            };
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

            var validationErrors = new List<string>();
            var customersQuery = _unitOfWork.Customers.GetQueryable()
                .Where(c => c.Id != id && !c.IsDeleted);

            var normalizedFullName = dto.FullName?.Trim();
            var normalizedPhone = dto.Phone?.Trim();
            var normalizedEmail = dto.Email?.Trim();
            var normalizedGender = dto.Gender?.Trim();
            var normalizedAddress = dto.Address?.Trim();
            var normalizedCardId = dto.CardId?.Trim();

            if (!string.IsNullOrEmpty(normalizedPhone))
            {
                var phoneExists = await customersQuery.AnyAsync(c => c.Phone == normalizedPhone);
                if (phoneExists)
                {
                    validationErrors.Add("Số điện thoại đã được sử dụng.");
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                var lowerEmail = normalizedEmail.ToLower();
                var emailExists = await customersQuery.AnyAsync(c => c.Email != null && c.Email.ToLower() == lowerEmail);
                if (emailExists)
                {
                    validationErrors.Add("Email đã được sử dụng.");
                }
            }

            if (!string.IsNullOrEmpty(normalizedCardId))
            {
                var cardExists = await customersQuery.AnyAsync(c => c.CardId == normalizedCardId);
                if (cardExists)
                {
                    validationErrors.Add("CCCD đã được sử dụng.");
                }
            }

            if (validationErrors.Any())
            {
                throw new CustomerValidationException(validationErrors);
            }

            if (dto.FullName != null) entity.FullName = normalizedFullName;
            if (dto.Phone != null) entity.Phone = normalizedPhone ?? string.Empty;
            if (dto.Email != null) entity.Email = normalizedEmail;
            if (dto.Gender != null) entity.Gender = normalizedGender;
            if (dto.Address != null) entity.Address = normalizedAddress;
            if (dto.Dob.HasValue) entity.Dob = DateTimeHelper.ToUtc(dto.Dob);
            if (dto.CardId != null) entity.CardId = normalizedCardId;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId;

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

        public async Task<CustomerResponse?> SearchCustomerByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var normalizedPhone = System.Text.RegularExpressions.Regex.Replace(phone, @"[\s\-\(\)]", "");

            var entity = await _unitOfWork.Customers.GetQueryable()
                .FirstOrDefaultAsync(x => 
                    x.Phone == phone || 
                    x.Phone == normalizedPhone ||
                    x.Phone.Contains(normalizedPhone));

            if (entity == null) return null;

            return _mapper.Map<CustomerResponse>(entity);
        }

        public IQueryable<Customer> GetQueryableForOData()
        {
            return _unitOfWork.Customers.GetQueryable()
                .Include(c => c.Quotations)
                .Include(c => c.Orders)
                .Include(c => c.TestDriveBookings)
                .Where(c => !c.IsDeleted);
        }
    }
}
