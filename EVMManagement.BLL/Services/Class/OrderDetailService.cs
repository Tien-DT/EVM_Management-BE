using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.OrderDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.OrderDetail;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public async Task<PagedResult<OrderDetailResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.OrderDetails.GetQueryable();
            var totalCount = await _unitOfWork.OrderDetails.CountAsync();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrderDetailResponse
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    VehicleVariantId = x.VehicleVariantId,
                    VehicleId = x.VehicleId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountPercent = x.DiscountPercent,
                    Note = x.Note,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<OrderDetailResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<OrderDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (entity == null) return null;

            return new OrderDetailResponse
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                VehicleVariantId = entity.VehicleVariantId,
                VehicleId = entity.VehicleId,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                DiscountPercent = entity.DiscountPercent,
                Note = entity.Note,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
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
