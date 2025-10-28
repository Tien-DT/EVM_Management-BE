using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.HandoverRecord;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class HandoverRecordService : IHandoverRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HandoverRecordService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<HandoverRecordResponseDto> CreateAsync(HandoverRecordCreateDto dto)
        {
            var entity = new HandoverRecord
            {
                OrderId = dto.OrderId,
                VehicleId = dto.VehicleId,
                TransportDetailId = dto.TransportDetailId,
                Notes = dto.Notes,
                HandoverDate = DateTimeHelper.ToUtc(dto.HandoverDate)
            };

            await _unitOfWork.HandoverRecords.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await GetByIdAsync(entity.Id) ?? throw new Exception("Failed to create HandoverRecord");
            return created;
        }

        public async Task<PagedResult<HandoverRecordResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.HandoverRecords.GetQueryableWithIncludes();
            var total = await _unitOfWork.HandoverRecords.CountAsync(x => !x.IsDeleted);

            var entities = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(e => _mapper.Map<HandoverRecordResponseDto>(e)).ToList();
            return PagedResult<HandoverRecordResponseDto>.Create(items, total, pageNumber, pageSize);
        }

        public async Task<HandoverRecordResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.HandoverRecords.GetByIdWithIncludesAsync(id);
            if (entity == null) return null;
            return _mapper.Map<HandoverRecordResponseDto>(entity);
        }

        public async Task<HandoverRecordResponseDto?> UpdateAsync(Guid id, HandoverRecordUpdateDto dto)
        {
            var entity = await _unitOfWork.HandoverRecords.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.TransportDetailId.HasValue) entity.TransportDetailId = dto.TransportDetailId.Value;
            if (dto.Notes != null) entity.Notes = dto.Notes;
            if (dto.IsAccepted.HasValue) entity.IsAccepted = dto.IsAccepted.Value;
            if (dto.HandoverDate.HasValue) entity.HandoverDate = DateTimeHelper.ToUtc(dto.HandoverDate);

            entity.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.HandoverRecords.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<HandoverRecordResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.HandoverRecords.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.DeletedDate = isDeleted ? DateTime.UtcNow : (DateTime?)null;

            _unitOfWork.HandoverRecords.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<HandoverRecordResponseDto> CreateHandoverWithVehicleAssignmentAsync(Guid orderId, OrderHandoverRequestDto dto)
        {
            // Lấy order với OrderDetails
            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            HandoverRecord? firstHandoverRecord = null;

            foreach (var orderDetail in order.OrderDetails.Where(od => !od.IsDeleted))
            {
                // Tìm vehicle có sẵn cho variant này
                var availableVehicle = await _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => v.VariantId == orderDetail.VehicleVariantId &&
                               v.Status == VehicleStatus.IN_STOCK &&
                               v.Purpose == VehiclePurpose.FOR_SALE &&
                               !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (availableVehicle == null)
                {
                    throw new Exception($"No available vehicle found for variant {orderDetail.VehicleVariantId}");
                }

                // gán xe vào OrderDetail
                orderDetail.VehicleId = availableVehicle.Id;
                orderDetail.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.OrderDetails.Update(orderDetail);

                // cập nhật trạng thái xe = SOLD
                availableVehicle.Status = VehicleStatus.SOLD;
                availableVehicle.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Vehicles.Update(availableVehicle);

                // tạo HandoverRecord
                var handoverRecord = new HandoverRecord
                {
                    OrderId = orderId,
                    VehicleId = availableVehicle.Id,
                    HandoverDate = dto.HandoverDate.HasValue 
                        ? DateTimeHelper.ToUtc(dto.HandoverDate) 
                        : DateTime.UtcNow,
                    Notes = dto.Notes,
                    IsAccepted = true
                };

                await _unitOfWork.HandoverRecords.AddAsync(handoverRecord);

                // lưu handover record đầu tiên để return
                if (firstHandoverRecord == null)
                {
                    firstHandoverRecord = handoverRecord;
                }
            }

            // cập nhật trạng thái đơn = COMPLETED
            order.Status = OrderStatus.COMPLETED;
            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);

            // log ghi nhận
            var report = new Report
            {
                Type = "HANDOVER_COMPLETED",
                Title = "Vehicle handover completed",
                Content = $"Order {order.Code} handover completed. {order.OrderDetails.Count} vehicle(s) assigned and status updated to SOLD.",
                OrderId = order.Id,
                DealerId = order.DealerId,
                AccountId = order.CreatedByUserId
            };
            await _unitOfWork.Reports.AddAsync(report);

            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(firstHandoverRecord!.Id) 
                ?? throw new Exception("Failed to create HandoverRecord");
        }

        public async Task<PagedResult<HandoverRecordResponseDto>> GetByFilterAsync(HandoverRecordFilterDto filter)
        {
            var query = _unitOfWork.HandoverRecords.GetQueryableWithIncludes();

            query = query.Where(x => !x.IsDeleted);

            if (filter.OrderId.HasValue)
                query = query.Where(x => x.OrderId == filter.OrderId.Value);

            if (filter.VehicleId.HasValue)
                query = query.Where(x => x.VehicleId == filter.VehicleId.Value);

            if (filter.TransportDetailId.HasValue)
                query = query.Where(x => x.TransportDetailId == filter.TransportDetailId.Value);

            if (filter.IsAccepted.HasValue)
                query = query.Where(x => x.IsAccepted == filter.IsAccepted.Value);

            var totalCount = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = entities.Select(e => _mapper.Map<HandoverRecordResponseDto>(e)).ToList();
            return PagedResult<HandoverRecordResponseDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public IQueryable<HandoverRecord> GetQueryableForOData()
        {
            return _unitOfWork.HandoverRecords.GetQueryable()
                .Include(h => h.Order)
                .Include(h => h.Vehicle)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Include(h => h.TransportDetail!)
                    .ThenInclude(td => td.Transport)
                .Where(h => !h.IsDeleted);
        }
    }
}
