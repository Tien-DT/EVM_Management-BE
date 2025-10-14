using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Class
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseCreateDto dto)
        {
            var entity = new Warehouse
            {
                DealerId = dto.DealerId,
                Name = dto.Name,
                Address = dto.Address,
                Capacity = dto.Capacity,
                Type = dto.Type
            };

            await _unitOfWork.Warehouses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new WarehouseResponseDto
            {
                Id = entity.Id,
                DealerId = entity.DealerId,
                Name = entity.Name,
                Address = entity.Address,
                Capacity = entity.Capacity,
                Type = entity.Type,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<PagedResult<WarehouseResponseDto>> GetAllWarehousesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Warehouses.GetQueryableWithIncludes();
            var totalCount = await _unitOfWork.Warehouses.CountAsync();

            var items = query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WarehouseResponseDto
                {
                    Id = x.Id,
                    DealerId = x.DealerId,
                    Name = x.Name,
                    Address = x.Address,
                    Capacity = x.Capacity,
                    Type = x.Type,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<WarehouseResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<WarehouseResponseDto?> GetWarehouseByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Warehouses.GetByIdWithIncludesAsync(id);
            if (entity == null) return null;

            return new WarehouseResponseDto
            {
                Id = entity.Id,
                DealerId = entity.DealerId,
                Name = entity.Name,
                Address = entity.Address,
                Capacity = entity.Capacity,
                Type = entity.Type,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<WarehouseResponseDto?> UpdateWarehouseAsync(Guid id, WarehouseUpdateDto dto)
        {
            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId;
            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Address != null) entity.Address = dto.Address;
            if (dto.Capacity.HasValue) entity.Capacity = dto.Capacity;
            entity.Type = dto.Type;

            _unitOfWork.Warehouses.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetWarehouseByIdAsync(id);
        }

        public async Task<WarehouseResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            _unitOfWork.Warehouses.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetWarehouseByIdAsync(id);
        }

        public async Task<bool> DeleteWarehouseAsync(Guid id)
        {
            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Warehouses.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
