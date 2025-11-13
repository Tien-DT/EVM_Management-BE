using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Dealer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EVMManagement.BLL.Services.Class
{
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DealerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dealer> CreateDealerAsync(CreateDealerDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Dữ liệu tạo đại lý không hợp lệ.");
            }

            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tên đại lý là bắt buộc.");
            }

            var email = dto.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email đại lý là bắt buộc.");
            }

            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Email đại lý không hợp lệ.");
            }

            var address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

            var lowerName = name.ToLower();
            var lowerEmail = email.ToLower();

            if (await _unitOfWork.Dealers.AnyAsync(d => !d.IsDeleted && d.Name.ToLower() == lowerName))
            {
                throw new ArgumentException($"Tên đại lý '{name}' đã tồn tại.");
            }

            if (await _unitOfWork.Dealers.AnyAsync(d => !d.IsDeleted && d.Email.ToLower() == lowerEmail))
            {
                throw new ArgumentException($"Email '{email}' đã được sử dụng.");
            }

            if (address != null)
            {
                var lowerAddress = address.ToLower();
                if (await _unitOfWork.Dealers.AnyAsync(d =>
                        !d.IsDeleted &&
                        d.Address != null &&
                        d.Address.ToLower() == lowerAddress))
                {
                    throw new ArgumentException("Địa chỉ đại lý đã được sử dụng.");
                }
            }

            if (phone != null)
            {
                if (!Regex.IsMatch(phone, @"^\d{10}$"))
                {
                    throw new ArgumentException("Số điện thoại đại lý phải gồm đúng 10 chữ số.");
                }

                if (await _unitOfWork.Dealers.AnyAsync(d => !d.IsDeleted && d.Phone == phone))
                {
                    throw new ArgumentException($"Số điện thoại '{phone}' đã được sử dụng.");
                }
            }

            var dealer = new Dealer
            {
                Name = name,
                Address = address,
                Phone = phone,
                Email = email,
                EstablishedAt = dto.EstablishedAt,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Dealers.AddAsync(dealer);
            await _unitOfWork.SaveChangesAsync();

            return dealer;
        }

        public Task<PagedResult<DealerResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null)
        {
            var query = _unitOfWork.Dealers.GetQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DealerResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    Phone = x.Phone,
                    Email = x.Email,
                    EstablishedAt = x.EstablishedAt,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<DealerResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<DealerResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            return new DealerResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                Phone = entity.Phone,
                Email = entity.Email,
                EstablishedAt = entity.EstablishedAt,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<DealerResponseDto?> UpdateAsync(Guid id, UpdateDealerDto dto)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Address != null) entity.Address = dto.Address;
            if (dto.Phone != null) entity.Phone = dto.Phone;
            if (dto.Email != null) entity.Email = dto.Email;
            if (dto.EstablishedAt.HasValue) entity.EstablishedAt = dto.EstablishedAt;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Dealers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<DealerResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Dealers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Dealers.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public IQueryable<Dealer> GetQueryableForOData()
        {
            return _unitOfWork.Dealers.GetQueryable()
                .Include(d => d.DealerContract)
                .Include(d => d.Warehouses)
                .Include(d => d.Orders)
                .Include(d => d.UserProfiles)
                .Where(d => !d.IsDeleted);
        }
    }
}

