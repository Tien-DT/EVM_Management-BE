using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVMManagement.BLL.Services.Class
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WarehouseService> _logger;
        private readonly IMapper _mapper;

        public WarehouseService(IUnitOfWork unitOfWork, ILogger<WarehouseService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
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
                Guid? finalDealerId = dto.DealerId;

                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    if (!currentUserDealerId.HasValue)
                    {
                        _logger.LogWarning("DEALER_MANAGER không có DealerId khi tạo warehouse. WarehouseName: {WarehouseName}", dto.Name);
                        return ApiResponse<WarehouseResponseDto>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ quản trị viên.", errorCode: 403);
                    }

                    if (dto.DealerId.HasValue && dto.DealerId.Value != currentUserDealerId.Value)
                    {
                        _logger.LogWarning("DEALER_MANAGER {DealerManagerDealerId} cố tạo warehouse cho dealer khác {RequestDealerId}", 
                            currentUserDealerId.Value, dto.DealerId.Value);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Bạn chỉ có thể tạo kho hàng cho dealer của mình. DealerId của bạn: {currentUserDealerId.Value}", errorCode: 403);
                    }

                    finalDealerId = currentUserDealerId.Value;
                }

                Dealer? dealer = null;
                if (finalDealerId.HasValue)
                {
                    dealer = await _unitOfWork.Dealers.GetByIdAsync(finalDealerId.Value);
                    if (dealer == null)
                    {
                        _logger.LogWarning("Cố tạo warehouse cho dealer không tồn tại. DealerId: {DealerId}", finalDealerId.Value);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer với ID '{finalDealerId.Value}' không tồn tại trong hệ thống.", errorCode: 404);
                    }

                    if (dealer.IsDeleted)
                    {
                        _logger.LogWarning("Cố tạo warehouse cho dealer đã bị xóa. DealerId: {DealerId}, DealerName: {DealerName}", 
                            finalDealerId.Value, dealer.Name);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer '{dealer.Name}' đã bị xóa. Vui lòng liên hệ quản trị viên.", errorCode: 400);
                    }

                    if (!dealer.IsActive)
                    {
                        _logger.LogWarning("Cố tạo warehouse cho dealer chưa active. DealerId: {DealerId}, DealerName: {DealerName}", 
                            finalDealerId.Value, dealer.Name);
                        return ApiResponse<WarehouseResponseDto>.CreateFail($"Dealer '{dealer.Name}' chưa được kích hoạt. Vui lòng liên hệ quản trị viên.", errorCode: 400);
                    }
                }

                var entity = new Warehouse
                {
                    DealerId = finalDealerId,
                    Name = dto.Name,
                    Address = dto.Address,
                    Capacity = dto.Capacity,
                    Type = dto.Type
                };

                await _unitOfWork.Warehouses.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var dealerName = dealer?.Name ?? "N/A";
                _logger.LogInformation("Warehouse được tạo thành công. CreatedBy: {CurrentUserRole}, WarehouseId: {WarehouseId}, DealerId: {DealerId}, DealerName: {DealerName}, WarehouseName: {WarehouseName}, Type: {Type}",
                    currentUserRole, entity.Id, finalDealerId, dealerName, dto.Name, dto.Type);

                var result = MapToDto(entity);
                var successMessage = finalDealerId.HasValue 
                    ? $"Tạo kho hàng '{dto.Name}' thành công cho dealer '{dealerName}'." 
                    : $"Tạo kho hàng '{dto.Name}' thành công.";
                return ApiResponse<WarehouseResponseDto>.CreateSuccess(result, successMessage);
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

        public async Task<PagedResult<WarehouseResponseDto>> GetWarehousesByTypeAsync(WarehouseType type, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Warehouses.GetWarehousesByType(type)
                .Where(x => !x.IsDeleted);

            var totalCount = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<WarehouseResponseDto>>(entities);

            return PagedResult<WarehouseResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<ApiResponse<PagedResult<VehicleResponseDto>>> GetVehiclesInEvmWarehouseAsync(
            Guid warehouseId,
            VehiclePurpose? purpose = null,
            VehicleStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetQueryable()
                    .FirstOrDefaultAsync(w => w.Id == warehouseId && !w.IsDeleted);

                if (warehouse == null)
                {
                    _logger.LogWarning("Không tìm thấy kho khi lấy xe EVM. WarehouseId: {WarehouseId}", warehouseId);
                    return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail("Không tìm thấy kho hoặc kho đã bị xóa.", errorCode: 404);
                }

                if (warehouse.Type != WarehouseType.EVM)
                {
                    _logger.LogWarning("Kho không thuộc loại EVM. WarehouseId: {WarehouseId}, Type: {Type}", warehouseId, warehouse.Type);
                    return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail("Kho này không phải kho của EVM.", errorCode: 400);
                }

                var query = _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => v.WarehouseId == warehouseId && !v.IsDeleted);

                if (purpose.HasValue)
                {
                    query = query.Where(v => v.Purpose == purpose.Value);
                }

                if (status.HasValue)
                {
                    query = query.Where(v => v.Status == status.Value);
                }

                var totalCount = await query.CountAsync();

                var vehicles = await query
                    .OrderByDescending(v => v.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = _mapper.Map<List<VehicleResponseDto>>(vehicles);
                var pagedResult = PagedResult<VehicleResponseDto>.Create(items, totalCount, pageNumber, pageSize);

                return ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(
                    pagedResult,
                    $"Lấy danh sách xe thuộc kho EVM '{warehouse.Name}' thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách xe trong kho EVM. WarehouseId: {WarehouseId}", warehouseId);
                return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail("Đã xảy ra lỗi khi lấy danh sách xe trong kho EVM.", errorCode: 500);
            }
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
                if (dto == null || dto.Vehicles == null || !dto.Vehicles.Any())
                {
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail("Danh sách xe cần thêm không được để trống.", errorCode: 400);
                }

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
                var vinSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var vehicle in dto.Vehicles)
                {
                    if (string.IsNullOrWhiteSpace(vehicle.Vin))
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail("Mỗi xe bắt buộc phải có số VIN.", errorCode: 400);
                    }

                    var trimmedVin = vehicle.Vin.Trim();
                    if (!vinSet.Add(trimmedVin))
                    {
                        return ApiResponse<List<VehicleResponseDto>>.CreateFail($"VIN '{trimmedVin}' đang bị trùng trong danh sách gửi lên.", errorCode: 400);
                    }
                }

                var normalizedRequestVins = vinSet.Select(v => v.ToUpperInvariant()).ToList();
                var existedVinList = await _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => !string.IsNullOrEmpty(v.Vin) && normalizedRequestVins.Contains(v.Vin.ToUpper()))
                    .Select(v => v.Vin)
                    .ToListAsync();

                if (existedVinList.Any())
                {
                    var duplicates = string.Join(", ", existedVinList.Distinct());
                    return ApiResponse<List<VehicleResponseDto>>.CreateFail($"Các VIN sau đã tồn tại trong hệ thống: {duplicates}", errorCode: 400);
                }

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

                    var vehicle = new Vehicle
                    {
                        VariantId = vehicleDto.VariantId,
                        WarehouseId = warehouse.Id,
                        Vin = vehicleDto.Vin.Trim().ToUpperInvariant(),
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
                        Vin = vehicleDto.Vin.Trim().ToUpperInvariant(),
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

        private WarehouseResponseDto MapToDto(Warehouse warehouse)
        {
            return _mapper.Map<WarehouseResponseDto>(warehouse);
        }

       
        public async Task<ApiResponse<PagedResult<WarehouseResponseDto>>> GetVehiclesInDealerWarehouseAsync(
            Guid warehouseId,
            VehiclePurpose? purpose = null,
            VehicleStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return ApiResponse<PagedResult<WarehouseResponseDto>>.CreateFail(
                        "PageNumber và PageSize phải lớn hơn 0.", errorCode: 400);
                }

                var warehouse = await _unitOfWork.Warehouses.GetQueryable()
                    .Include(w => w.Vehicles)
                        .ThenInclude(v => v.VehicleVariant)
                            .ThenInclude(vv => vv.VehicleModel)
                    .Include(w => w.Dealer)
                    .FirstOrDefaultAsync(w => w.Id == warehouseId && !w.IsDeleted);

                if (warehouse == null)
                {
                    _logger.LogWarning("Kho Dealer không tồn tại hoặc đã bị xóa. WarehouseId: {WarehouseId}", warehouseId);
                    return ApiResponse<PagedResult<WarehouseResponseDto>>.CreateFail(
                        $"Kho Dealer với ID '{warehouseId}' không tồn tại.", errorCode: 404);
                }

               
                var filteredVehicles = (warehouse.Vehicles ?? new List<Vehicle>())
                    .Where(v => !v.IsDeleted)
                    .Where(v => !purpose.HasValue || v.Purpose == purpose.Value)
                    .Where(v => !status.HasValue || v.Status == status.Value)
                    .OrderByDescending(v => v.CreatedDate)
                    .ToList();

                var totalCount = filteredVehicles.Count;

                var paginatedVehicles = filteredVehicles
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                
                warehouse.Vehicles = paginatedVehicles;

                var items = new List<WarehouseResponseDto> { _mapper.Map<WarehouseResponseDto>(warehouse) };
                var pagedResult = PagedResult<WarehouseResponseDto>.Create(items, totalCount, pageNumber, pageSize);

                return ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(
                    pagedResult,
                    $"Lấy danh sách xe thuộc kho Dealer '{warehouse.Name}' thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách xe trong kho Dealer. WarehouseId: {WarehouseId}", warehouseId);
                return ApiResponse<PagedResult<WarehouseResponseDto>>.CreateFail(
                    "Đã xảy ra lỗi khi lấy danh sách xe trong kho Dealer.", errorCode: 500);
            }
        }

        public async Task<ApiResponse<PagedResult<VehicleResponseDto>>> GetVehiclesByModelInWarehouseAsync(Guid warehouseId, Guid modelId, VehiclePurpose? purpose = null, VehicleStatus? status = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail(
                        "PageNumber và PageSize phải lớn hơn 0.", errorCode: 400);
                }

                // Verify warehouse exists
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
                if (warehouse == null || warehouse.IsDeleted)
                {
                    return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail(
                        "Kho không tồn tại hoặc đã bị xóa.", errorCode: 404);
                }

                // Verify vehicle model exists
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(modelId);
                if (model == null || model.IsDeleted)
                {
                    return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail(
                        "Mẫu xe không tồn tại hoặc đã bị xóa.", errorCode: 404);
                }

                var query = _unitOfWork.Vehicles.GetVehiclesByModelInWarehouseAsync(warehouseId, modelId, purpose, status);

                var totalCount = await query.CountAsync();

                var vehicles = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = _mapper.Map<List<VehicleResponseDto>>(vehicles);
                var pagedResult = PagedResult<VehicleResponseDto>.Create(items, totalCount, pageNumber, pageSize);

                var purposeText = purpose.HasValue ? $" ({purpose.Value})" : "";
                var statusText = status.HasValue ? $" - {status.Value}" : "";
                var message = $"Lấy danh sách xe {model.Name}{purposeText}{statusText} trong kho '{warehouse.Name}' thành công.";

                return ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(pagedResult, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách xe theo model trong warehouse. WarehouseId: {WarehouseId}, ModelId: {ModelId}", warehouseId, modelId);
                return ApiResponse<PagedResult<VehicleResponseDto>>.CreateFail(
                    "Đã xảy ra lỗi khi lấy danh sách xe.", errorCode: 500);
            }
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


