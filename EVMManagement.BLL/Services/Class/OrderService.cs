using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Order> CreateOrderAsync(OrderCreateDto dto)
        {
            var order = new Order
            {
                Code = dto.Code,
                QuotationId = dto.QuotationId,
                CustomerId = dto.CustomerId,
                DealerId = dto.DealerId,
                CreatedByUserId = dto.CreatedByUserId,
                Status = dto.Status,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = dto.DiscountAmount,
                FinalAmount = dto.FinalAmount,
                ExpectedDeliveryAt = dto.ExpectedDeliveryAt,
                OrderType = dto.OrderType,
                IsFinanced = dto.IsFinanced
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order;
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Orders.GetQueryable();
            var totalCount = await _unitOfWork.Orders.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<OrderResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<OrderResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<OrderResponse>(entity);
        }

        public async Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateDto dto)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.QuotationId.HasValue) entity.QuotationId = dto.QuotationId;
            if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;
            if (dto.TotalAmount.HasValue) entity.TotalAmount = dto.TotalAmount;
            if (dto.DiscountAmount.HasValue) entity.DiscountAmount = dto.DiscountAmount;
            if (dto.FinalAmount.HasValue) entity.FinalAmount = dto.FinalAmount;
            if (dto.ExpectedDeliveryAt.HasValue) entity.ExpectedDeliveryAt = dto.ExpectedDeliveryAt;
            if (dto.OrderType.HasValue) entity.OrderType = dto.OrderType.Value;
            if (dto.IsFinanced.HasValue) entity.IsFinanced = dto.IsFinanced.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Orders.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<OrderResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Orders.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Orders.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Orders.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
