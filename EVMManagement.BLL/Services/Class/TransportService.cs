using System;
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
            }

            await _unitOfWork.SaveChangesAsync();

            // Return the created transport with details
            return await GetByIdAsync(transport.Id) 
                ?? throw new Exception("Failed to retrieve created transport");
        }

        public async Task<PagedResult<TransportResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Transports.GetQueryable()
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.VehicleVariant)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Order)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(t => new TransportResponseDto
            {
                Id = t.Id,
                ProviderName = t.ProviderName,
                PickupLocation = t.PickupLocation,
                DropoffLocation = t.DropoffLocation,
                Status = t.Status,
                ScheduledPickupAt = t.ScheduledPickupAt,
                DeliveredAt = t.DeliveredAt,
                CreatedDate = t.CreatedDate,
                ModifiedDate = t.ModifiedDate,
                TransportDetails = t.TransportDetails.Select(td => new TransportDetailDto
                {
                    Id = td.Id,
                    TransportId = td.TransportId,
                    VehicleId = td.VehicleId,
                    OrderId = td.OrderId,
                    VehicleVin = td.Vehicle?.Vin,
                    VehicleVariantName = td.Vehicle?.VehicleVariant?.Name,
                    OrderCode = td.Order?.Code
                }).ToList()
            }).ToList();

            return new PagedResult<TransportResponseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<TransportResponseDto?> GetByIdAsync(Guid id)
        {
            var transport = await _unitOfWork.Transports.GetQueryable()
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Vehicle)
                        .ThenInclude(v => v.VehicleVariant)
                .Include(t => t.TransportDetails)
                    .ThenInclude(td => td.Order)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (transport == null) return null;

            return new TransportResponseDto
            {
                Id = transport.Id,
                ProviderName = transport.ProviderName,
                PickupLocation = transport.PickupLocation,
                DropoffLocation = transport.DropoffLocation,
                Status = transport.Status,
                ScheduledPickupAt = transport.ScheduledPickupAt,
                DeliveredAt = transport.DeliveredAt,
                CreatedDate = transport.CreatedDate,
                ModifiedDate = transport.ModifiedDate,
                TransportDetails = transport.TransportDetails.Select(td => new TransportDetailDto
                {
                    Id = td.Id,
                    TransportId = td.TransportId,
                    VehicleId = td.VehicleId,
                    OrderId = td.OrderId,
                    VehicleVin = td.Vehicle?.Vin,
                    VehicleVariantName = td.Vehicle?.VehicleVariant?.Name,
                    OrderCode = td.Order?.Code
                }).ToList()
            };
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
    }
}

