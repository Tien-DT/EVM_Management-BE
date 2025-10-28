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
            // Validate that all orders exist, are B2B, and are IN_PROGRESS
            var orders = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Vehicle)
                .Where(o => dto.OrderIds.Contains(o.Id))
                .ToListAsync();

            if (orders.Count != dto.OrderIds.Count)
            {
                throw new Exception("One or more orders not found");
            }

            foreach (var order in orders)
            {
                if (order.OrderType != OrderType.B2B)
                {
                    throw new Exception($"Order {order.Code} is not a B2B order");
                }

                if (order.Status != OrderStatus.IN_PROGRESS)
                {
                    throw new Exception($"Order {order.Code} is not in IN_PROGRESS status");
                }
            }

            // Create Transport
            var transport = new Transport
            {
                ProviderName = dto.ProviderName,
                PickupLocation = dto.PickupLocation,
                DropoffLocation = dto.DropoffLocation,
                ScheduledPickupAt = dto.ScheduledPickupAt,
                Status = TransportStatus.PENDING,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Transports.AddAsync(transport);
            await _unitOfWork.SaveChangesAsync();

            // Create TransportDetails for each vehicle in the orders
            foreach (var order in orders)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.VehicleId.HasValue)
                    {
                        var transportDetail = new TransportDetail
                        {
                            TransportId = transport.Id,
                            VehicleId = orderDetail.VehicleId.Value,
                            OrderId = order.Id,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _unitOfWork.TransportDetails.AddAsync(transportDetail);
                    }
                }

                // Update Order status to IN_TRANSIT
                order.Status = OrderStatus.IN_TRANSIT;
                order.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);
            }

            await _unitOfWork.SaveChangesAsync();

            // Return the created transport with details
            return await GetByIdAsync(transport.Id) 
                ?? throw new Exception("Failed to retrieve created transport");
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

        public Task<PagedResult<TransportResponseDto>> GetByDealerAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var filter = new TransportFilterDto
            {
                DealerId = dealerId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return GetAllAsync(filter);
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

            transport.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Transports.Update(transport);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
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
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.VehicleVariant)
                            .ThenInclude(vv => vv.VehicleModel)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Order)
                        .ThenInclude(o => o!.Dealer)
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

            if (filter.DealerId.HasValue)
            {
                var dealerId = filter.DealerId.Value;
                query = query.Where(t => t.TransportDetails.Any(td => td.Order != null && td.Order!.DealerId == dealerId));
            }

            if (filter.OrderId.HasValue)
            {
                var orderId = filter.OrderId.Value;
                query = query.Where(t => t.TransportDetails.Any(td => td.OrderId == orderId));
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

