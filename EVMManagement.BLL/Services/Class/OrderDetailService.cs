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
