using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EVMManagement.BLL.Helpers;
using Microsoft.EntityFrameworkCore;
using EVMManagement.BLL.DTOs.Request.Transport;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Transport;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class TransportService : ITransportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TransportResponseDto> CreateAsync(TransportCreateDto dto)
        {
            Order? order = null;

            if (dto.OrderId.HasValue)
            {
                order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Vehicle)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrderId.Value);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã {dto.OrderId.Value}");
                }

                if (order.OrderType != OrderType.B2B)
                {
                    throw new InvalidOperationException($"Đơn hàng {order.Code} không phải loại B2B");
                }

                if (order.Status != OrderStatus.IN_PROGRESS)
                {
                    throw new InvalidOperationException($"Đơn hàng {order.Code} phải ở trạng thái IN_PROGRESS (hiện tại: {order.Status})");
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var transport = new Transport
                {
                    ProviderName = dto.ProviderName,
                    PickupLocation = dto.PickupLocation,
                    DropoffLocation = dto.DropoffLocation,
                    ScheduledPickupAt = dto.ScheduledPickupAt,
                    OrderId = dto.OrderId,
                    Status = TransportStatus.PENDING,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Transports.AddAsync(transport);
                await _unitOfWork.SaveChangesAsync();

                if (order != null)
                {
                    order.Status = OrderStatus.IN_TRANSIT;
                    order.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                var result = await GetByIdAsync(transport.Id);
                if (result == null)
                {
                    throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển vừa tạo");
                }

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Xảy ra lỗi khi tạo vận chuyển: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<TransportResponseDto>> GetAllAsync(TransportFilterDto? filter = null)
        {
            filter ??= new TransportFilterDto();
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var query = ApplyFilter(BuildTransportQuery(), filter)
                .OrderByDescending(t => t.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<TransportResponseDto>>(items);

            return PagedResult<TransportResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<TransportResponseDto>> GetByDealerAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var pageNumberValue = pageNumber < 1 ? 1 : pageNumber;
            var pageSizeValue = pageSize < 1 ? 10 : pageSize;

            var query = BuildTransportQuery()
                .Where(t => t.Order != null && t.Order.DealerId == dealerId)
                .OrderByDescending(t => t.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumberValue - 1) * pageSizeValue)
                .Take(pageSizeValue)
                .ToListAsync();

            var dtos = _mapper.Map<List<TransportResponseDto>>(items);

            return PagedResult<TransportResponseDto>.Create(dtos, totalCount, pageNumberValue, pageSizeValue);
        }

        public Task<PagedResult<TransportResponseDto>> GetByOrderAsync(Guid orderId, int pageNumber = 1, int pageSize = 10)
        {
            var filter = new TransportFilterDto
            {
                OrderId = orderId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return GetAllAsync(filter);
        }

        public async Task<TransportResponseDto?> GetByIdAsync(Guid id)
        {
            var transport = await BuildTransportQuery()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transport == null) return null;

            return _mapper.Map<TransportResponseDto>(transport);
        }

        public async Task<TransportResponseDto?> UpdateAsync(Guid id, TransportUpdateDto dto)
        {
            var transport = await BuildTransportQuery()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transport == null) return null;

            var previousStatus = transport.Status;

            if (dto.ProviderName != null) transport.ProviderName = dto.ProviderName;
            if (dto.PickupLocation != null) transport.PickupLocation = dto.PickupLocation;
            if (dto.DropoffLocation != null) transport.DropoffLocation = dto.DropoffLocation;
            if (dto.Status.HasValue) transport.Status = dto.Status.Value;
            if (dto.ScheduledPickupAt.HasValue) transport.ScheduledPickupAt = DateTimeHelper.ToUtc(dto.ScheduledPickupAt);
            if (dto.DeliveredAt.HasValue) transport.DeliveredAt = DateTimeHelper.ToUtc(dto.DeliveredAt);
            if (dto.OrderId.HasValue)
            {
                var orderExists = await _unitOfWork.Orders.GetQueryable()
                    .AnyAsync(o => o.Id == dto.OrderId.Value && !o.IsDeleted);
                if (!orderExists)
                {
                    throw new KeyNotFoundException("Không tìm thấy đơn hàng cần cập nhật");
                }
                transport.OrderId = dto.OrderId.Value;
            }

            var isDeliveredState = dto.Status.HasValue
                && dto.Status.Value == TransportStatus.DELIVERED
                && previousStatus != TransportStatus.DELIVERED;

            transport.ModifiedDate = DateTime.UtcNow;

            if (!isDeliveredState)
            {
                _unitOfWork.Transports.Update(transport);
                await _unitOfWork.SaveChangesAsync();
                return await GetByIdAsync(id);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await CreateHandoverRecordsForTransportAsync(transport);

                _unitOfWork.Transports.Update(transport);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return await GetByIdAsync(id);
        }

        public async Task<TransportResponseDto> CancelAsync(Guid id)
        {
            var transport = await _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transport == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy vận chuyển với mã {id}");
            }

            if (transport.Status == TransportStatus.CANCELED)
            {
                var canceled = await GetByIdAsync(id);
                if (canceled == null)
                {
                    throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển sau khi hủy");
                }

                return canceled;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                transport.Status = TransportStatus.CANCELED;
                transport.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Transports.Update(transport);

                var vehiclesToUpdate = transport.TransportDetails
                    .Where(td => td.Vehicle != null)
                    .Select(td => td.Vehicle!)
                    .GroupBy(v => v.Id)
                    .Select(g => g.First())
                    .ToList();

                foreach (var vehicle in vehiclesToUpdate)
                {
                    vehicle.Status = VehicleStatus.IN_STOCK;
                    vehicle.ModifiedDate = DateTime.UtcNow;
                }

                if (vehiclesToUpdate.Count > 0)
                {
                    _unitOfWork.Vehicles.UpdateRange(vehiclesToUpdate);
                }

                if (transport.Order != null)
                {
                    transport.Order.Status = OrderStatus.IN_PROGRESS;
                    transport.Order.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(transport.Order);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Xảy ra lỗi khi hủy vận chuyển: {ex.Message}", ex);
            }

            var result = await GetByIdAsync(id);
            if (result == null)
            {
                throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển sau khi hủy");
            }

            return result;
        }

        public async Task<TransportResponseDto> ConfirmHandoverAsync(Guid transportId)
        {
            var transport = await _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
                .Include(t => t.HandoverRecords.Where(hr => !hr.IsDeleted))
                .FirstOrDefaultAsync(t => t.Id == transportId && !t.IsDeleted);

            if (transport == null)
            {
                throw new KeyNotFoundException("Không tìm thấy vận chuyển với mã yêu cầu");
            }

            if (transport.Status != TransportStatus.DELIVERED)
            {
                throw new InvalidOperationException("Chỉ xác nhận bàn giao cho vận chuyển đang ở trạng thái DELIVERED");
            }

            var handoverRecord = transport.HandoverRecords.FirstOrDefault();
            if (handoverRecord == null)
            {
                throw new InvalidOperationException("Chưa có biên bản bàn giao cho vận chuyển này");
            }

            if (handoverRecord.IsAccepted)
            {
                var current = await GetByIdAsync(transportId);
                if (current == null)
                {
                    throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển sau khi xác nhận bàn giao");
                }
                return current;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                handoverRecord.IsAccepted = true;
                handoverRecord.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.HandoverRecords.Update(handoverRecord);

                transport.Status = TransportStatus.COMPLETED;
                transport.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Transports.Update(transport);

                if (transport.OrderId.HasValue)
                {
                    var order = transport.Order;
                    if (order == null)
                    {
                        order = await _unitOfWork.Orders.GetByIdAsync(transport.OrderId.Value);
                    }

                    if (order == null)
                    {
                        throw new InvalidOperationException("Kh�ng t�m th?y don h�ng li�n quan v?i v?n chuy?n");
                    }

                    order.Status = OrderStatus.HANDOVER_SUCCESS;
                    order.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Xảy ra lỗi khi xác nhận bàn giao: {ex.Message}", ex);
            }

            var result = await GetByIdAsync(transportId);
            if (result == null)
            {
                throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển sau khi xác nhận bàn giao");
            }

            return result;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var transport = await _unitOfWork.Transports.GetByIdAsync(id);
            if (transport == null) return false;

            transport.IsDeleted = true;
            transport.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Transports.Update(transport);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public IQueryable<Transport> GetQueryableForOData()
        {
            return BuildTransportQuery();
        }

        public async Task<TransportResponseDto> AddTransportToWarehouseAsync(AddTransportToWarehouseDto dto)
        {
            var transport = await _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.Warehouse)
                .FirstOrDefaultAsync(t => t.Id == dto.TransportId && !t.IsDeleted);

            if (transport == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy vận chuyển với mã {dto.TransportId}");
            }

            if (transport.Status == TransportStatus.END)
            {
                throw new InvalidOperationException("Vận chuyển đã kết thúc, không thể thêm xe vào kho");
            }

            var warehouse = await _unitOfWork.Warehouses.GetQueryable()
                .Include(w => w.Dealer)
                .FirstOrDefaultAsync(w => w.Id == dto.WarehouseId && !w.IsDeleted);

            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy kho hàng với mã {dto.WarehouseId}");
            }

            if (warehouse.DealerId.HasValue && warehouse.Dealer != null && !warehouse.Dealer.IsActive)
            {
                throw new InvalidOperationException("Kho hàng thuộc đại lý không hoạt động, không thể thêm xe vào kho");
            }

            var transportDetails = transport.TransportDetails
                .Where(td => !td.IsDeleted && td.Vehicle != null)
                .ToList();

            if (transportDetails.Count == 0)
            {
                throw new InvalidOperationException("Vận chuyển không có xe nào để thêm vào kho");
            }

            Order? order = null;
            if (transport.OrderId.HasValue)
            {
                order = transport.Order ?? await _unitOfWork.Orders.GetQueryable()
                    .FirstOrDefaultAsync(o => o.Id == transport.OrderId.Value && !o.IsDeleted);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã {transport.OrderId.Value}");
                }
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var detail in transportDetails)
                {
                    var vehicle = detail.Vehicle;
                    if (vehicle == null) continue;

                    vehicle.WarehouseId = warehouse.Id;
                    vehicle.Status = VehicleStatus.IN_STOCK;
                    vehicle.ModifiedDate = DateTime.UtcNow;

                    _unitOfWork.Vehicles.Update(vehicle);
                }

                transport.Status = TransportStatus.END;
                transport.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Transports.Update(transport);

                if (order != null)
                {
                    order.Status = OrderStatus.COMPLETED;
                    order.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Xảy ra lỗi khi thêm xe vào kho: {ex.Message}", ex);
            }

            var result = await GetByIdAsync(transport.Id);
            if (result == null)
            {
                throw new InvalidOperationException("Không thể truy xuất thông tin vận chuyển sau khi thêm xe vào kho");
            }

            return result;
        }

        private async Task CreateHandoverRecordsForTransportAsync(Transport transport)
        {
            if (!transport.OrderId.HasValue)
            {
                throw new InvalidOperationException("Vận chuyển chưa được gán đơn hàng nên không thể chuyển sang trạng thái DELIVERED");
            }

            var details = transport.TransportDetails
                .Where(td => !td.IsDeleted)
                .ToList();

            if (details.Count == 0)
            {
                throw new InvalidOperationException("Vận chuyển chưa có xe để bàn giao");
            }

            transport.HandoverRecords ??= new List<HandoverRecord>();

            if (transport.HandoverRecords.Any(hr => !hr.IsDeleted))
            {
                throw new InvalidOperationException("Đã tồn tại biên bản bàn giao cho chuyến vận chuyển này");
            }

            if (details.Any(td => td.Vehicle == null))
            {
                throw new InvalidOperationException("Thông tin xe không hợp lệ, không thể tạo biên bản bàn giao");
            }

            var order = transport.Order;
            if (order == null)
            {
                order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.HandoverRecord)
                    .FirstOrDefaultAsync(o => o.Id == transport.OrderId.Value);
            }

            if (order == null)
            {
                throw new InvalidOperationException("Không tìm thấy đơn hàng gắn với vận chuyển");
            }

            if (order.HandoverRecord != null && !order.HandoverRecord.IsDeleted)
            {
                throw new InvalidOperationException("Đơn hàng đã có biên bản bàn giao");
            }

            transport.Order = order;

            var vehicles = details
                .Where(td => td.Vehicle != null)
                .Select(td => td.Vehicle!)
                .GroupBy(v => v.Id)
                .Select(g => g.First())
                .ToList();

            var primaryDetail = details
                .OrderBy(td => td.CreatedDate)
                .First();

            if (primaryDetail.VehicleId == Guid.Empty)
            {
                throw new InvalidOperationException("Xe trong chuyến vận chuyển không hợp lệ");
            }

            var handoverDate = transport.DeliveredAt ?? DateTime.UtcNow;
            var normalizedHandoverDate = DateTimeHelper.ToUtc(handoverDate);
            transport.DeliveredAt = normalizedHandoverDate;

            var handoverRecord = new HandoverRecord
            {
                OrderId = transport.OrderId.Value,
                VehicleId = primaryDetail.VehicleId,
                TransportId = transport.Id,
                HandoverDate = normalizedHandoverDate,
                IsAccepted = false,
                Notes = "Tạo tự động khi vận chuyển hoàn tất"
            };

            await _unitOfWork.HandoverRecords.AddAsync(handoverRecord);
            order.HandoverRecord = handoverRecord;
            transport.HandoverRecords.Add(handoverRecord);

            foreach (var vehicle in vehicles)
            {
                vehicle.Status = VehicleStatus.SOLD;
                vehicle.ModifiedDate = DateTime.UtcNow;
            }

            if (vehicles.Count > 0)
            {
                _unitOfWork.Vehicles.UpdateRange(vehicles);
            }

            order.Status = OrderStatus.COMPLETED;
            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);
        }

        private IQueryable<Transport> BuildTransportQuery()
        {
            return _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
                    .ThenInclude(o => o!.Dealer)
                .Include(t => t.Order)
                    .ThenInclude(o => o!.HandoverRecord)
                .Include(t => t.HandoverRecords)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.VehicleVariant)
                            .ThenInclude(vv => vv.VehicleModel)
                .Where(t => !t.IsDeleted);
        }

        private static IQueryable<Transport> ApplyFilter(IQueryable<Transport> query, TransportFilterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.ProviderName))
            {
                var providerName = filter.ProviderName.Trim();
                query = query.Where(t => t.ProviderName != null && EF.Functions.Like(t.ProviderName, $"%{providerName}%"));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.OrderId.HasValue)
            {
                var orderId = filter.OrderId.Value;
                query = query.Where(t => t.OrderId == orderId);
            }

            if (filter.CreatedFrom.HasValue)
            {
                var from = filter.CreatedFrom.Value;
                query = query.Where(t => t.CreatedDate >= from);
            }

            if (filter.CreatedTo.HasValue)
            {
                var to = filter.CreatedTo.Value;
                query = query.Where(t => t.CreatedDate <= to);
            }

            return query;
        }
    }
}






