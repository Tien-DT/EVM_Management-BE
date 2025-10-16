using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleResponseDto> CreateVehicleAsync(VehicleCreateDto dto)
        {
            var entity = new Vehicle
            {
                VariantId = dto.VariantId,
                WarehouseId = dto.WarehouseId,
                Vin = dto.Vin,
                Status = dto.Status,
                Purpose = dto.Purpose
            };

            await _unitOfWork.Vehicles.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<PagedResult<VehicleResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Vehicles.GetQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();

            return PagedResult<VehicleResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        public async Task<VehicleResponseDto?> UpdateAsync(Guid id, VehicleUpdateDto dto)
        {
            var entity = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.VariantId.HasValue) entity.VariantId = dto.VariantId.Value;
            if (dto.WarehouseId.HasValue) entity.WarehouseId = dto.WarehouseId.Value;
            if (!string.IsNullOrWhiteSpace(dto.Vin)) entity.Vin = dto.Vin!;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;
            if (dto.Purpose.HasValue) entity.Purpose = dto.Purpose.Value;

            _unitOfWork.Vehicles.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<VehicleResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            entity.DeletedDate = isDeleted ? DateTime.UtcNow : null;
            _unitOfWork.Vehicles.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        

        public async Task<PagedResult<VehicleResponseDto>> SearchByQueryAsync(string? q, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return await GetAllAsync(pageNumber, pageSize);
            }

            var query = _unitOfWork.Vehicles.SearchByQueryAsync(q!);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();

            return PagedResult<VehicleResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        
        

        public async Task<PagedResult<VehicleResponseDto>> GetByFilterAsync(VehicleFilterDto filter)
        {
            var query = _unitOfWork.Vehicles.GetQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                var q = filter.Q.ToLower();
                query = query.Where(v => v.Vin.ToLower().Contains(q));
            }

            if (filter.Status.HasValue) query = query.Where(v => v.Status == filter.Status.Value);
            if (filter.Purpose.HasValue) query = query.Where(v => v.Purpose == filter.Purpose.Value);
            if (filter.WarehouseId.HasValue) query = query.Where(v => v.WarehouseId == filter.WarehouseId.Value);
            if (filter.VariantId.HasValue) query = query.Where(v => v.VariantId == filter.VariantId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();
            return PagedResult<VehicleResponseDto>.Create(responses, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<VehicleResponseDto?> UpdateStatusAsync(Guid id, VehicleStatus status)
        {
            var entity = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Status = status;
            _unitOfWork.Vehicles.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        private VehicleResponseDto MapToDto(Vehicle e)
        {
            return new VehicleResponseDto
            {
                Id = e.Id,
                VariantId = e.VariantId,
                WarehouseId = e.WarehouseId,
                Vin = e.Vin,
                Status = e.Status,
                Purpose = e.Purpose,
                CreatedDate = e.CreatedDate,
                ModifiedDate = e.ModifiedDate,
                DeletedDate = e.DeletedDate,
                IsDeleted = e.IsDeleted
            };
        }
    }
}
