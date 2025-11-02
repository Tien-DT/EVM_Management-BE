using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
            var transport = await _unitOfWork.Transports.GetByIdAsync(id);
            if (transport == null) return null;

            if (dto.ProviderName != null) transport.ProviderName = dto.ProviderName;
            if (dto.PickupLocation != null) transport.PickupLocation = dto.PickupLocation;
            if (dto.DropoffLocation != null) transport.DropoffLocation = dto.DropoffLocation;
            if (dto.Status.HasValue) transport.Status = dto.Status.Value;
            if (dto.ScheduledPickupAt.HasValue) transport.ScheduledPickupAt = dto.ScheduledPickupAt;
            if (dto.DeliveredAt.HasValue) transport.DeliveredAt = dto.DeliveredAt;
            if (dto.OrderId.HasValue)
            {
                var orderExists = await _unitOfWork.Orders.AnyAsync(o => o.Id == dto.OrderId.Value && !o.IsDeleted);
                if (!orderExists)
                {
                    throw new KeyNotFoundException("Không tìm thấy đơn hàng cần cập nhật");
                }
                transport.OrderId = dto.OrderId.Value;
            }

            transport.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Transports.Update(transport);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<TransportResponseDto> CancelAsync(Guid id)
        {
            var transport = await _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
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

        private IQueryable<Transport> BuildTransportQuery()
        {
            return _unitOfWork.Transports.GetQueryable()
                .Include(t => t.Order)
                    .ThenInclude(o => o!.Dealer)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.VehicleVariant)
                            .ThenInclude(vv => vv.VehicleModel)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.HandoverRecord)
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






