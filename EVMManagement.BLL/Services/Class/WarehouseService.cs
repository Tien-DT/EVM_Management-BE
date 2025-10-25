using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;
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
                    return ApiResponse<WarehouseResponseDto>.CreateFail("DealerId là bắt buộc khi tạo warehouse.", errorCode: 400);
                }

                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    if (!currentUserDealerId.HasValue)
                    {
                        _logger.LogWarning("DEALER_MANAGER không có DealerId khi tạo warehouse. WarehouseName: {WarehouseName}", dto.Name);
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ quản trị viên.", errorCode: 403);
                    }

                    if (dto.DealerId.Value != currentUserDealerId.Value)
                    {
                        _logger.LogWarning("DEALER_MANAGER {DealerManagerDealerId} cố tạo warehouse cho dealer khác {RequestDealerId}", 
                            currentUserDealerId.Value, dto.DealerId.Value);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Bạn chỉ có thể tạo kho hàng cho dealer của mình. DealerId của bạn: {currentUserDealerId.Value}", errorCode: 403);
                    }
                }

                var dealer = await _unitOfWork.Dealers.GetByIdAsync(dto.DealerId.Value);
                if (dealer == null)
                {
                    _logger.LogWarning("Cố tạo warehouse cho dealer không tồn tại. DealerId: {DealerId}", dto.DealerId.Value);
                    return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer với ID '{dto.DealerId.Value}' không tồn tại trong hệ thống.", errorCode: 404);
                }

                if (dealer.IsDeleted)
                {
                    _logger.LogWarning("Cố tạo warehouse cho dealer đã bị xóa. DealerId: {DealerId}, DealerName: {DealerName}", 
                        dto.DealerId.Value, dealer.Name);
                    return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer '{dealer.Name}' đã bị xóa. Vui lòng liên hệ quản trị viên.", errorCode: 400);
                }

                if (!dealer.IsActive)
                {
                    _logger.LogWarning("Cố tạo warehouse cho dealer chưa active. DealerId: {DealerId}, DealerName: {DealerName}", 
                        dto.DealerId.Value, dealer.Name);
                    return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer '{dealer.Name}' chưa được kích hoạt. Vui lòng liên hệ quản trị viên.", errorCode: 400);
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

                _logger.LogInformation("Warehouse được tạo thành công. CreatedBy: {CurrentUserRole}, WarehouseId: {WarehouseId}, DealerId: {DealerId}, DealerName: {DealerName}, WarehouseName: {WarehouseName}, Type: {Type}",
                    currentUserRole, entity.Id, dto.DealerId, dealer.Name, dto.Name, dto.Type);

                var result = MapToDto(entity);
                return ApiResponse<WarehouseResponseDto>.CreateSuccess(result, $"Tạo kho hàng '{dto.Name}' thành công cho dealer '{dealer.Name}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi tạo warehouse. CurrentUserRole: {CurrentUserRole}, DealerId: {DealerId}, WarehouseName: {WarehouseName}",
                    currentUserRole, dto?.DealerId, dto?.Name);
                return ApiResponse<WarehouseResponseDto>.CreateFail("Có lỗi xảy ra khi tạo kho hàng. Vui lòng thử lại sau hoặc liên hệ quản trị viên.", errorCode: 500);
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

        public async Task<ApiResponse<WarehouseResponseDto>> UpdateWarehouseAsync(Guid id, WarehouseUpdateDto dto, AccountRole currentUserRole, Guid? currentUserDealerId = null)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Yêu cầu không hợp lệ.");
                }

            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse không tồn tại.", errorCode: 404);
                }

                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    if (!currentUserDealerId.HasValue)
                    {
                        _logger.LogWarning("DEALER_MANAGER không có DealerId khi update warehouse. WarehouseId: {WarehouseId}", id);
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ quản trị viên.", errorCode: 403);
                    }

                    if (entity.DealerId != currentUserDealerId.Value)
                    {
                        _logger.LogWarning("DEALER_MANAGER {DealerManagerDealerId} cố update warehouse của dealer khác. WarehouseId: {WarehouseId}, WarehouseDealerId: {WarehouseDealerId}",
                            currentUserDealerId.Value, id, entity.DealerId);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Bạn chỉ có thể cập nhật kho hàng của dealer mình. Warehouse này thuộc DealerId: {entity.DealerId}", errorCode: 403);
                    }
                }

                var oldName = entity.Name;
                if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId.Value;
            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Address != null) entity.Address = dto.Address;
            if (dto.Capacity.HasValue) entity.Capacity = dto.Capacity;
            entity.Type = dto.Type;
                entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Warehouses.Update(entity);
            await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Warehouse được cập nhật thành công. UpdatedBy: {CurrentUserRole}, WarehouseId: {WarehouseId}, DealerId: {DealerId}, OldName: '{OldName}', NewName: '{NewName}'",
                    currentUserRole, id, entity.DealerId, oldName, entity.Name);

                var result = await GetWarehouseByIdAsync(id);
                return ApiResponse<WarehouseResponseDto>.CreateSuccess(result, $"Cập nhật kho hàng '{entity.Name}' thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi cập nhật warehouse. CurrentUserRole: {CurrentUserRole}, WarehouseId: {WarehouseId}",
                    currentUserRole, id);
                return ApiResponse<WarehouseResponseDto>.CreateFail("Có lỗi xảy ra khi cập nhật kho hàng. Vui lòng thử lại sau hoặc liên hệ quản trị viên.", errorCode: 500);
            }
        }

        public async Task<ApiResponse<WarehouseResponseDto>> UpdateIsDeletedAsync(Guid id, bool isDeleted, AccountRole currentUserRole, Guid? currentUserDealerId = null)
        {
            try
        {
            var entity = await _unitOfWork.Warehouses.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse không tồn tại.", errorCode: 404);
                }

                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    if (!currentUserDealerId.HasValue)
                    {
                        _logger.LogWarning("DEALER_MANAGER không có DealerId khi soft delete warehouse. WarehouseId: {WarehouseId}", id);
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ quản trị viên.", errorCode: 403);
                    }

                    if (entity.DealerId != currentUserDealerId.Value)
                    {
                        _logger.LogWarning("DEALER_MANAGER {DealerManagerDealerId} cố soft delete warehouse của dealer khác. WarehouseId: {WarehouseId}, WarehouseDealerId: {WarehouseDealerId}",
                            currentUserDealerId.Value, id, entity.DealerId);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Bạn chỉ có thể xóa kho hàng của dealer mình. Warehouse này thuộc DealerId: {entity.DealerId}", errorCode: 403);
                    }
                }

                var warehouseName = entity.Name;
            entity.IsDeleted = isDeleted;
                if (isDeleted)
                {
                    entity.DeletedDate = DateTime.UtcNow;
                }
                entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Warehouses.Update(entity);
            await _unitOfWork.SaveChangesAsync();

                var action = isDeleted ? "xóa (soft delete)" : "khôi phục";
                _logger.LogInformation("Warehouse được {Action} thành công. UpdatedBy: {CurrentUserRole}, WarehouseId: {WarehouseId}, DealerId: {DealerId}, WarehouseName: '{WarehouseName}', IsDeleted: {IsDeleted}",
                    action, currentUserRole, id, entity.DealerId, warehouseName, isDeleted);

                var result = await GetWarehouseByIdAsync(id);
                var message = isDeleted ? $"Xóa kho hàng '{warehouseName}' thành công." : $"Khôi phục kho hàng '{warehouseName}' thành công.";
                return ApiResponse<WarehouseResponseDto>.CreateSuccess(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi cập nhật IsDeleted của warehouse. CurrentUserRole: {CurrentUserRole}, WarehouseId: {WarehouseId}, IsDeleted: {IsDeleted}",
                    currentUserRole, id, isDeleted);
                return ApiResponse<WarehouseResponseDto>.CreateFail("Có lỗi xảy ra khi cập nhật trạng thái kho hàng. Vui lòng thử lại sau hoặc liên hệ quản trị viên.", errorCode: 500);
            }
        }

        public async Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToWarehouseAsync(Guid warehouseId, AddVehiclesToWarehouseRequestDto dto, Guid addedByUserId)
        {
            try
            {
                // Kiểm tra warehouse tồn tại
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
                if (warehouse == null || warehouse.IsDeleted)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Warehouse not found or has been deleted", errorCode: 404);
                }

                // kiểm tra variant tồn tại
                var variant = await _unitOfWork.VehicleVariants.GetByIdAsync(dto.VehicleVariantId);
                if (variant == null || variant.IsDeleted)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Vehicle variant not found or has been deleted", errorCode: 404);
                }

                var createdVehicles = new List<VehicleResponseDto>();

                // tạo vehicle cho từng VIN
                foreach (var vin in dto.VinNumbers)
                {
                    if (string.IsNullOrWhiteSpace(vin))
                    {
                        _logger.LogWarning("Empty VIN number detected, skipping...");
                        continue;
                    }

                    // kiểm tra VIN tồn tại
                    var existingVehicle = await _unitOfWork.Vehicles.GetQueryable()
                        .Where(v => v.Vin == vin)
                        .FirstOrDefaultAsync();

                    if (existingVehicle != null)
                    {
                        _logger.LogWarning("VIN {VIN} already exists, skipping...", vin);
                        continue;
                    }

                    var vehicle = new Vehicle
                    {
                        VariantId = dto.VehicleVariantId,
                        WarehouseId = warehouseId,
                        Vin = vin,
                        Status = VehicleStatus.IN_STOCK,
                        Purpose = dto.Purpose
                    };

                    await _unitOfWork.Vehicles.AddAsync(vehicle);

                    createdVehicles.Add(new VehicleResponseDto
                    {
                        Id = vehicle.Id,
                        VariantId = vehicle.VariantId,
                        WarehouseId = vehicle.WarehouseId,
                        Vin = vehicle.Vin,
                        ImageUrl = vehicle.ImageUrl,
                        Status = vehicle.Status,
                        Purpose = vehicle.Purpose,
                        CreatedDate = vehicle.CreatedDate,
                        ModifiedDate = vehicle.ModifiedDate,
                        DeletedDate = vehicle.DeletedDate,
                        IsDeleted = vehicle.IsDeleted
                    });
                }

                // report log
                var report = new Report
                {
                    Type = "VEHICLES_ADDED_TO_WAREHOUSE",
                    Title = "Vehicles added to warehouse",
                    Content = $"{createdVehicles.Count} vehicle(s) added to warehouse {warehouse.Name}. Variant: {variant.Color}",
                    DealerId = warehouse.DealerId,
                    AccountId = addedByUserId
                };
                await _unitOfWork.Reports.AddAsync(report);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("{Count} vehicles added to warehouse {WarehouseId} by user {UserId}", 
                    createdVehicles.Count, warehouseId, addedByUserId);

                return ApiResponse<List<VehicleResponseDto>>.CreateSuccess(
                    createdVehicles, 
                    $"Successfully added {createdVehicles.Count} vehicle(s) to warehouse {warehouse.Name}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicles to warehouse {WarehouseId}", warehouseId);
                return ApiResponse<List<VehicleResponseDto>>.CreateFail("An error occurred while adding vehicles to warehouse", errorCode: 500);
            }
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