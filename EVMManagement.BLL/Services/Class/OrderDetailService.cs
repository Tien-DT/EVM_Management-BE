using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.OrderDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.OrderDetail;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetailCreateDto dto)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = dto.OrderId,
                VehicleVariantId = dto.VehicleVariantId,
                VehicleId = dto.VehicleId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                DiscountPercent = dto.DiscountPercent,
                Note = dto.Note
            };

            await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            await _unitOfWork.SaveChangesAsync();

            return orderDetail;
        }

        public async Task<List<OrderDetail>> CreateOrderDetailsAsync(List<OrderDetailCreateDto> dtos)
        {
            var orderDetails = new List<OrderDetail>();

            foreach (var dto in dtos)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = dto.OrderId,
                    VehicleVariantId = dto.VehicleVariantId,
                    VehicleId = dto.VehicleId,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    DiscountPercent = dto.DiscountPercent,
                    Note = dto.Note
                };
                orderDetails.Add(orderDetail);
            }

            await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
            await _unitOfWork.SaveChangesAsync();

            return orderDetails;
        }

        public async Task<OrderDetailBulkCreateResponse> CreateOrderDetailsV2Async(List<OrderDetailCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                throw new ArgumentException("Danh sách chi tiết đơn hàng không được để trống");
            }

            var orderId = dtos[0].OrderId;
            if (dtos.Any(d => d.OrderId != orderId))
            {
                throw new ArgumentException("Tất cả chi tiết đơn hàng phải thuộc cùng một đơn hàng");
            }

            var order = await _unitOfWork.Orders.GetQueryable()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã {orderId}");
            }

            var existingDetails = await _unitOfWork.OrderDetails.GetQueryable()
                .Where(od => od.OrderId == orderId && !od.IsDeleted)
                .ToListAsync();

            var orderDetails = dtos.Select(dto => new OrderDetail
            {
                OrderId = dto.OrderId,
                VehicleVariantId = dto.VehicleVariantId,
                VehicleId = dto.VehicleId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                DiscountPercent = dto.DiscountPercent,
                Note = dto.Note
            }).ToList();

            var vehicleIds = orderDetails
                .Where(d => d.VehicleId.HasValue)
                .Select(d => d.VehicleId!.Value)
                .Distinct()
                .ToList();

            List<Vehicle> vehicles = new List<Vehicle>();
            if (vehicleIds.Count > 0)
            {
                vehicles = await _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => vehicleIds.Contains(v.Id))
                    .ToListAsync();

                if (vehicles.Count != vehicleIds.Count)
                {
                    throw new KeyNotFoundException("Không tìm thấy một hoặc nhiều xe trong kho");
                }

                foreach (var vehicle in vehicles)
                {
                    if (vehicle.Status == VehicleStatus.SOLD)
                    {
                        throw new InvalidOperationException($"Xe {vehicle.Id} đã được bán");
                    }
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (orderDetails.Count > 0)
                {
                    await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
                }

                if (vehicles.Count > 0)
                {
                    foreach (var vehicle in vehicles)
                    {
                        vehicle.Status = VehicleStatus.SOLD;
                        vehicle.ModifiedDate = DateTime.UtcNow;
                    }

                    _unitOfWork.Vehicles.UpdateRange(vehicles);
                }

                var combinedDetails = existingDetails.Concat(orderDetails).ToList();

                var totalAmount = Math.Round(combinedDetails.Sum(d => d.UnitPrice * d.Quantity), 2);
                var discountAmount = Math.Round(combinedDetails.Sum(d => d.UnitPrice * d.Quantity * (d.DiscountPercent / 100m)), 2);
                var finalAmount = Math.Round(totalAmount - discountAmount, 2);
                if (finalAmount < 0)
                {
                    finalAmount = 0;
                }

                order.TotalAmount = totalAmount;
                order.DiscountAmount = discountAmount;
                order.FinalAmount = finalAmount;
                order.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            var createdIds = orderDetails.Select(od => od.Id).ToList();

            var createdEntities = await _unitOfWork.OrderDetails.GetQueryable()
                .Where(od => createdIds.Contains(od.Id))
                .Include(od => od.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Include(od => od.Vehicle)
                .ToListAsync();

            var response = new OrderDetailBulkCreateResponse
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                CreatedOrderDetails = _mapper.Map<List<OrderDetailResponse>>(createdEntities)
            };

            return response;
        }

        public async Task<PagedResult<OrderDetailResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.OrderDetails.GetQueryable();
            var totalCount = await _unitOfWork.OrderDetails.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<OrderDetailResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<OrderDetailResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<OrderDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<OrderDetailResponse>(entity);
        }

        public async Task<OrderDetailResponse?> UpdateAsync(Guid id, OrderDetailUpdateDto dto)
        {
            var entity = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.OrderId.HasValue) entity.OrderId = dto.OrderId.Value;
            if (dto.VehicleVariantId.HasValue) entity.VehicleVariantId = dto.VehicleVariantId.Value;
            if (dto.VehicleId.HasValue) entity.VehicleId = dto.VehicleId;
            if (dto.Quantity.HasValue) entity.Quantity = dto.Quantity.Value;
            if (dto.UnitPrice.HasValue) entity.UnitPrice = dto.UnitPrice.Value;
            if (dto.DiscountPercent.HasValue) entity.DiscountPercent = dto.DiscountPercent.Value;
            if (dto.Note != null) entity.Note = dto.Note;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.OrderDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<OrderDetailResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.OrderDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.OrderDetails.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
