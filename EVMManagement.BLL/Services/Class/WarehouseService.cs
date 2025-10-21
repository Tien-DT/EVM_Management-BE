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
using Microsoft.Extensions.Logging;

namespace EVMManagement.BLL.Services.Class
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(IUnitOfWork unitOfWork, ILogger<WarehouseService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<WarehouseResponseDto>> CreateWarehouseAsync(WarehouseCreateDto dto, AccountRole currentUserRole, Guid? currentUserDealerId = null)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Yêu cầu không hợp lệ.");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Tên kho là bắt buộc.");
                }

                if (!dto.DealerId.HasValue)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("DealerId là bắt buộc.", errorCode: 400);
                }

                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    // DEALER_MANAGER chỉ được tạo warehouse cho dealer của mình
                    if (!currentUserDealerId.HasValue)
                    {
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Không tìm thấy thông tin dealer của bạn.", errorCode: 403);
                    }

                    if (dto.DealerId.Value != currentUserDealerId.Value)
                    {
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Bạn chỉ có thể tạo kho hàng cho dealer của mình.", errorCode: 403);
                    }
                }

                // validate dealer tồn tại 
                var dealer = await _unitOfWork.Dealers.GetByIdAsync(dto.DealerId.Value);
                if (dealer == null)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Dealer không tồn tại.", errorCode: 404);
                }

                if (dealer.IsDeleted)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Dealer đã bị xóa.", errorCode: 400);
                }

                if (!dealer.IsActive)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Dealer chưa được kích hoạt.", errorCode: 400);
                }

                var entity = new Warehouse
                {
                    DealerId = dto.DealerId.Value,
                    Name = dto.Name,
                    Address = dto.Address,
                    Capacity = dto.Capacity,
                    Type = dto.Type
                };

                await _unitOfWork.Warehouses.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Warehouse được tạo thành công bởi {CurrentUserRole} cho dealer {DealerId}: {WarehouseName}",
                    currentUserRole, dto.DealerId, dto.Name);

                var result = MapToDto(entity);
                return ApiResponse<WarehouseResponseDto>.CreateSuccess(result, "Tạo kho hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo warehouse. CurrentUserRole: {CurrentUserRole}, DealerId: {DealerId}",
                    currentUserRole, dto?.DealerId);
                return ApiResponse<WarehouseResponseDto>.CreateFail(ex);
            }
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