using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

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

            return MapToDto(entity);
        }

        public async Task<PagedResult<WarehouseResponseDto>> GetAllWarehousesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Warehouses.GetAllAsync();
            var totalCount = await _unitOfWork.Warehouses.CountAsync();

            var entities = query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = entities.Select(MapToDto).ToList();

            return PagedResult<WarehouseResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public Task<PagedResult<WarehouseResponseDto>> GetWarehousesByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Warehouses.GetWarehousesByDealerIdAsync(dealerId);
            
            var totalCount = query.Count(x => !x.IsDeleted);

            var entities = query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = entities.Select(MapToDto).ToList();

            var result = PagedResult<WarehouseResponseDto>.Create(items, totalCount, pageNumber, pageSize);
            return Task.FromResult(result);
        }

        public async Task<WarehouseResponseDto?> GetWarehouseByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (entity == null) return null;

            return MapToDto(entity);
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

       
        private static WarehouseResponseDto MapToDto(Warehouse w)
        {
            return new WarehouseResponseDto
            {
                Id = w.Id,
                DealerId = w.DealerId,
                Name = w.Name,
                Address = w.Address,
                Capacity = w.Capacity,
                Type = w.Type,
                Dealer = w.Dealer == null ? null : new DealerDto
                {
                    Id = w.Dealer.Id,
                    Name = w.Dealer.Name
                },
                Vehicles = w.Vehicles?.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    VariantId = v.VariantId,
                    Vin = v.Vin,
                    Status = v.Status,
                    Variant = v.VehicleVariant == null ? null : new VehicleVariantDto
                    {
                        Color = v.VehicleVariant.Color,
                        VehicleModel = v.VehicleVariant.VehicleModel == null ? null : new VehicleModelDto
                        {
                            Name = v.VehicleVariant.VehicleModel.Name,
                            Ranking = (VehicleModelRanking)v.VehicleVariant.VehicleModel.Ranking
                        }
                    }
                }).ToList(),
                CreatedDate = w.CreatedDate,
                ModifiedDate = w.ModifiedDate,
                DeletedDate = w.DeletedDate,
                IsDeleted = w.IsDeleted
            };
        }
    }
}