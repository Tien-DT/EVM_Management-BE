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

        public async Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToWarehouseAsync(AddVehiclesToWarehouseRequestDto dto, Guid addedByUserId)
        {
            try
            {
                if (!dto.WarehouseId.HasValue && !dto.DealerId.HasValue)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Phải cung cấp WarehouseId hoặc DealerId", errorCode: 400);
                }

                Warehouse? warehouse = null;

                if (dto.WarehouseId.HasValue)
                {
                    warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId.Value);
                    if (warehouse == null || warehouse.IsDeleted)
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail("Không tìm thấy kho hoặc kho đã bị xóa", errorCode: 404);
                    }
                }
                else if (dto.DealerId.HasValue)
                {
                    var warehouses = await _unitOfWork.Warehouses.GetQueryable()
                        .Where(w => w.DealerId == dto.DealerId.Value && !w.IsDeleted)
                        .OrderBy(w => w.CreatedDate)
                        .ToListAsync();

                    if (!warehouses.Any())
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail($"Không tìm thấy kho nào cho dealer với ID '{dto.DealerId.Value}'", errorCode: 404);
                    }

                    warehouse = warehouses.First();
                    _logger.LogInformation("Sử dụng kho mặc định {WarehouseId} cho dealer {DealerId}", warehouse.Id, dto.DealerId.Value);
                }

                var variant = await _unitOfWork.VehicleVariants.GetByIdAsync(dto.VehicleVariantId);
                if (variant == null || variant.IsDeleted)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Không tìm thấy biến thể xe hoặc biến thể đã bị xóa", errorCode: 404);
                }

                var createdVehicles = new List<VehicleResponseDto>();

                foreach (var vin in dto.VinNumbers)
                {
                    if (string.IsNullOrWhiteSpace(vin))
                    {
                        _logger.LogWarning("Phát hiện số VIN trống, bỏ qua...");
                        continue;
                    }

                    var existingVehicle = await _unitOfWork.Vehicles.GetQueryable()
                        .Where(v => v.Vin == vin)
                        .FirstOrDefaultAsync();

                    if (existingVehicle != null)
                    {
                        _logger.LogWarning("VIN {VIN} đã tồn tại, bỏ qua...", vin);
                        continue;
                    }

                    var vehicle = new Vehicle
                    {
                        VariantId = dto.VehicleVariantId,
                        WarehouseId = warehouse!.Id,
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

                var report = new Report
                {
                    Type = "VEHICLES_ADDED_TO_WAREHOUSE",
                    Title = "Xe đã được thêm vào kho",
                    Content = $"{createdVehicles.Count} xe đã được thêm vào kho {warehouse.Name}. Biến thể: {variant.Color}",
                    DealerId = warehouse.DealerId,
                    AccountId = addedByUserId
                };
                await _unitOfWork.Reports.AddAsync(report);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("{Count} xe đã được thêm vào kho {WarehouseId} bởi user {UserId}", 
                    createdVehicles.Count, warehouse.Id, addedByUserId);

                return ApiResponse<List<VehicleResponseDto>>.CreateSuccess(
                    createdVehicles, 
                    $"Đã thêm thành công {createdVehicles.Count} xe vào kho {warehouse.Name}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm xe vào kho");
                return ApiResponse<List<VehicleResponseDto>>.CreateFail("Đã xảy ra lỗi khi thêm xe vào kho", errorCode: 500);
            }
        }

        public async Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToEvmWarehouseAsync(AddVehiclesToEvmWarehouseDto dto, Guid addedByUserId)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
                if (warehouse == null || warehouse.IsDeleted)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Không tìm thấy kho hoặc kho đã bị xóa", errorCode: 404);
                }

                if (warehouse.Type != WarehouseType.EVM)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Kho này không phải là kho của EVM", errorCode: 400);
                }

                var createdVehicles = new List<VehicleResponseDto>();
                var variantIds = dto.Vehicles.Select(v => v.VariantId).Distinct().ToList();
                var variants = await _unitOfWork.VehicleVariants.GetQueryable()
                    .Where(v => variantIds.Contains(v.Id) && !v.IsDeleted)
                    .ToDictionaryAsync(v => v.Id, v => v);

                foreach (var vehicleDto in dto.Vehicles)
                {
                    if (!variants.ContainsKey(vehicleDto.VariantId))
                    {
                        _logger.LogWarning("Biến thể xe {VariantId} không tồn tại, bỏ qua...", vehicleDto.VariantId);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(vehicleDto.Vin))
                    {
                        _logger.LogWarning("Phát hiện số VIN trống, bỏ qua...");
                        continue;
                    }

                    var existingVehicle = await _unitOfWork.Vehicles.GetQueryable()
                        .Where(v => v.Vin == vehicleDto.Vin)
                        .FirstOrDefaultAsync();

                    if (existingVehicle != null)
                    {
                        _logger.LogWarning("VIN {VIN} đã tồn tại, bỏ qua...", vehicleDto.Vin);
                        continue;
                    }

                    var vehicle = new Vehicle
                    {
                        VariantId = vehicleDto.VariantId,
                        WarehouseId = warehouse.Id,
                        Vin = vehicleDto.Vin,
                        Status = vehicleDto.Status,
                        Purpose = vehicleDto.Purpose,
                        ImageUrl = vehicleDto.ImageUrl
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

                var report = new Report
                {
                    Type = "VEHICLES_ADDED_TO_EVM_WAREHOUSE",
                    Title = "Xe đã được thêm vào kho EVM",
                    Content = $"{createdVehicles.Count} xe đã được thêm vào kho EVM {warehouse.Name}",
                    AccountId = addedByUserId
                };
                await _unitOfWork.Reports.AddAsync(report);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("{Count} xe đã được thêm vào kho EVM {WarehouseId} bởi user {UserId}", 
                    createdVehicles.Count, warehouse.Id, addedByUserId);

                return ApiResponse<List<VehicleResponseDto>>.CreateSuccess(
                    createdVehicles, 
                    $"Đã thêm thành công {createdVehicles.Count} xe vào kho EVM {warehouse.Name}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm xe vào kho EVM");
                return ApiResponse<List<VehicleResponseDto>>.CreateFail("Đã xảy ra lỗi khi thêm xe vào kho EVM", errorCode: 500);
            }
        }

        public async Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToDealerWarehouseAsync(AddVehiclesToDealerWarehouseDto dto, Guid addedByUserId)
        {
            try
            {
                if (!dto.WarehouseId.HasValue && !dto.DealerId.HasValue)
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Phải cung cấp WarehouseId hoặc DealerId", errorCode: 400);
                }

                Warehouse? warehouse = null;

                if (dto.WarehouseId.HasValue)
                {
                    warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId.Value);
                    if (warehouse == null || warehouse.IsDeleted)
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail("Không tìm thấy kho hoặc kho đã bị xóa", errorCode: 404);
                    }

                    if (warehouse.Type != WarehouseType.DEALER)
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail("Kho này không phải là kho của Dealer", errorCode: 400);
                    }
                }
                else if (dto.DealerId.HasValue)
                {
                    var warehouses = await _unitOfWork.Warehouses.GetQueryable()
                        .Where(w => w.DealerId == dto.DealerId.Value && w.Type == WarehouseType.DEALER && !w.IsDeleted)
                        .OrderBy(w => w.CreatedDate)
                        .ToListAsync();

                    if (!warehouses.Any())
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail($"Không tìm thấy kho Dealer nào cho dealer với ID '{dto.DealerId.Value}'", errorCode: 404);
                    }

                    warehouse = warehouses.First();
                    _logger.LogInformation("Sử dụng kho Dealer mặc định {WarehouseId} cho dealer {DealerId}", warehouse.Id, dto.DealerId.Value);
                }

                var createdVehicles = new List<VehicleResponseDto>();
                var variantIds = dto.Vehicles.Select(v => v.VariantId).Distinct().ToList();
                var variants = await _unitOfWork.VehicleVariants.GetQueryable()
                    .Where(v => variantIds.Contains(v.Id) && !v.IsDeleted)
                    .ToDictionaryAsync(v => v.Id, v => v);

                foreach (var vehicleDto in dto.Vehicles)
                {
                    if (!variants.ContainsKey(vehicleDto.VariantId))
                    {
                        _logger.LogWarning("Biến thể xe {VariantId} không tồn tại, bỏ qua...", vehicleDto.VariantId);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(vehicleDto.Vin))
                    {
                        _logger.LogWarning("Phát hiện số VIN trống, bỏ qua...");
                        continue;
                    }

                    var existingVehicle = await _unitOfWork.Vehicles.GetQueryable()
                        .Where(v => v.Vin == vehicleDto.Vin)
                        .FirstOrDefaultAsync();

                    if (existingVehicle != null)
                    {
                        _logger.LogWarning("VIN {VIN} đã tồn tại, bỏ qua...", vehicleDto.Vin);
                        continue;
                    }

                    var vehicle = new Vehicle
                    {
                        VariantId = vehicleDto.VariantId,
                        WarehouseId = warehouse!.Id,
                        Vin = vehicleDto.Vin,
                        Status = vehicleDto.Status,
                        Purpose = vehicleDto.Purpose,
                        ImageUrl = vehicleDto.ImageUrl
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

                var report = new Report
                {
                    Type = "VEHICLES_ADDED_TO_DEALER_WAREHOUSE",
                    Title = "Xe đã được thêm vào kho Dealer",
                    Content = $"{createdVehicles.Count} xe đã được thêm vào kho Dealer {warehouse.Name}",
                    DealerId = warehouse.DealerId,
                    AccountId = addedByUserId
                };
                await _unitOfWork.Reports.AddAsync(report);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("{Count} xe đã được thêm vào kho Dealer {WarehouseId} bởi user {UserId}", 
                    createdVehicles.Count, warehouse.Id, addedByUserId);

                return ApiResponse<List<VehicleResponseDto>>.CreateSuccess(
                    createdVehicles, 
                    $"Đã thêm thành công {createdVehicles.Count} xe vào kho Dealer {warehouse.Name}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm xe vào kho Dealer");
                return ApiResponse<List<VehicleResponseDto>>.CreateFail("Đã xảy ra lỗi khi thêm xe vào kho Dealer", errorCode: 500);
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

        public IQueryable<Warehouse> GetQueryableForOData()
        {
            return _unitOfWork.Warehouses.GetQueryable()
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Where(w => !w.IsDeleted);
        }
    }
}