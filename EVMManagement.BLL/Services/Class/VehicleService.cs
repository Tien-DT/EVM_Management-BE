using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VehicleResponseDto> CreateVehicleAsync(VehicleCreateDto dto)
        {
            var entity = new Vehicle
            {
                VariantId = dto.VariantId,
                WarehouseId = dto.WarehouseId,
                Vin = dto.Vin,
                Status = dto.Status,
                Purpose = dto.Purpose,
                ImageUrl = dto.ImageUrl
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
                .ProjectTo<VehicleResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleDetailResponseDto>> GetAllWithDetailsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Vehicles.GetQueryable()
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Include(v => v.Warehouse)
                    .ThenInclude(w => w.Dealer);
            
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<VehicleDetailResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleDetailResponseDto>.Create(items, totalCount, pageNumber, pageSize);
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
            if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;

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
                .ProjectTo<VehicleResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleResponseDto>.Create(items, totalCount, pageNumber, pageSize);
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
                .ProjectTo<VehicleResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleResponseDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
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

        public async Task<StockCheckResponseDto> CheckStockAvailabilityAsync(Guid variantId, Guid dealerId, int quantity)
        {
            // Get warehouses belonging to the dealer
            var warehouses = await _unitOfWork.Warehouses.GetQueryable()
                .Where(w => w.DealerId == dealerId && !w.IsDeleted)
                .ToListAsync();

            var warehouseIds = warehouses.Select(w => w.Id).ToList();

            // Query available vehicles
            var availableVehicles = await _unitOfWork.Vehicles.GetQueryable()
                .Where(v => v.VariantId == variantId &&
                           warehouseIds.Contains(v.WarehouseId) &&
                           v.Status == VehicleStatus.IN_STOCK &&
                           v.Purpose == VehiclePurpose.FOR_SALE &&
                           !v.IsDeleted)
                .ToListAsync();

            var availableQuantity = availableVehicles.Count;
            var isInStock = availableQuantity >= quantity;

            // Get primary warehouse (first one or null)
            var primaryWarehouse = warehouses.FirstOrDefault();

            return new StockCheckResponseDto
            {
                IsInStock = isInStock,
                AvailableQuantity = availableQuantity,
                RequestedQuantity = quantity,
                WarehouseId = primaryWarehouse?.Id,
                WarehouseName = primaryWarehouse?.Name,
                AvailableVehicleIds = availableVehicles.Select(v => v.Id).ToList()
            };
        }

        public async Task<List<DealerModelListDto>> GetModelsByDealerAsync(Guid dealerId)
        {
            var models = await _unitOfWork.Vehicles.GetModelsByDealerAsync(dealerId);

            var result = models.Select(m => new DealerModelListDto
            {
                Id = m.Id,
                Name = m.Name,
                VariantCount = m.VariantCount
            }).ToList();

            return result;
        }

        public async Task<List<DealerVariantListDto>> GetVariantsByDealerAndModelAsync(Guid dealerId, Guid modelId)
        {
            var variants = await _unitOfWork.Vehicles.GetVariantsByDealerAndModelAsync(dealerId, modelId);

            var result = variants.Select(v => new DealerVariantListDto
            {
                Id = v.Id,
                ModelId = v.ModelId,
                ModelName = v.ModelName,
                AvailableCount = v.AvailableCount
            }).ToList();

            return result;
        }

        private VehicleResponseDto MapToDto(Vehicle e)
        {
            return _mapper.Map<VehicleResponseDto>(e);
        }
    }
}
