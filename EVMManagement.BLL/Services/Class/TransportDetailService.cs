using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.TransportDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TransportDetail;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class TransportDetailService : ITransportDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransportDetailService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<TransportDetailResponseDto>> CreateAsync(List<TransportDetailCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                throw new ArgumentException("Danh sách chi tiết vận chuyển không được để trống");
            }

            var duplicatedVehicles = dtos
                .GroupBy(x => x.VehicleId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedVehicles.Count > 0)
            {
                throw new InvalidOperationException($"Xe với mã {string.Join(", ", duplicatedVehicles)} bị trùng trong yêu cầu");
            }

            var transportIds = dtos.Select(x => x.TransportId).Distinct().ToList();
            var validTransportIds = await _unitOfWork.Transports.GetQueryable()
                .Where(t => transportIds.Contains(t.Id) && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();

            if (validTransportIds.Count != transportIds.Count)
            {
                var missingIds = transportIds.Except(validTransportIds).ToList();
                throw new KeyNotFoundException($"Không tìm thấy vận chuyển với mã: {string.Join(", ", missingIds)}");
            }

            var vehicleIds = dtos.Select(x => x.VehicleId).Distinct().ToList();
            var vehicles = await _unitOfWork.Vehicles.GetQueryable()
                .Include(v => v.TransportDetail)
                .Where(v => vehicleIds.Contains(v.Id))
                .ToListAsync();

            if (vehicles.Count != vehicleIds.Count)
            {
                var foundIds = vehicles.Select(v => v.Id).ToList();
                var missingVehicleIds = vehicleIds.Except(foundIds).ToList();
                throw new KeyNotFoundException($"Không tìm thấy xe với mã: {string.Join(", ", missingVehicleIds)}");
            }

            var vehiclesWithTransport = vehicles
                .Where(v => v.TransportDetail != null)
                .Select(v => v.Id)
                .ToList();
            if (vehiclesWithTransport.Count > 0)
            {
                throw new InvalidOperationException($"Xe với mã {string.Join(", ", vehiclesWithTransport)} đã được gán cho chuyến vận chuyển khác");
            }

            var orderIds = dtos.Where(x => x.OrderId.HasValue).Select(x => x.OrderId!.Value).Distinct().ToList();
            if (orderIds.Count > 0)
            {
                var validOrderIds = await _unitOfWork.Orders.GetQueryable()
                    .Where(o => orderIds.Contains(o.Id) && !o.IsDeleted)
                    .Select(o => o.Id)
                    .ToListAsync();

                if (validOrderIds.Count != orderIds.Count)
                {
                    var missingOrderIds = orderIds.Except(validOrderIds).ToList();
                    throw new KeyNotFoundException($"Không tìm thấy đơn hàng với mã: {string.Join(", ", missingOrderIds)}");
                }
            }

            var entities = dtos.Select(dto => new TransportDetail
            {
                TransportId = dto.TransportId,
                VehicleId = dto.VehicleId,
                OrderId = dto.OrderId,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            await _unitOfWork.TransportDetails.AddRangeAsync(entities);
            await _unitOfWork.SaveChangesAsync();

            var createdIds = entities.Select(e => e.Id).ToList();

            var createdEntities = await _unitOfWork.TransportDetails.GetQueryable()
                .Where(td => createdIds.Contains(td.Id))
                .Include(td => td.Vehicle)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Include(td => td.Order)
                .ToListAsync();

            return _mapper.Map<List<TransportDetailResponseDto>>(createdEntities);
        }

        public async Task<PagedResult<TransportDetailResponseDto>> GetAllAsync(TransportDetailFilterDto filter)
        {
            filter ??= new TransportDetailFilterDto();

            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                throw new ArgumentException("PageNumber và PageSize phải lớn hơn 0");
            }

            var query = _unitOfWork.TransportDetails.GetQueryable();

            if (filter.IsDeleted.HasValue)
            {
                query = query.Where(td => td.IsDeleted == filter.IsDeleted.Value);
            }
            else
            {
                query = query.Where(td => !td.IsDeleted);
            }

            if (filter.TransportId.HasValue)
            {
                query = query.Where(td => td.TransportId == filter.TransportId.Value);
            }

            if (filter.VehicleId.HasValue)
            {
                query = query.Where(td => td.VehicleId == filter.VehicleId.Value);
            }

            if (filter.OrderId.HasValue)
            {
                query = query.Where(td => td.OrderId == filter.OrderId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(td => td.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<TransportDetailResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<TransportDetailResponseDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<TransportDetailResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.TransportDetails.GetQueryable()
                .Where(td => td.Id == id)
                .ProjectTo<TransportDetailResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return entity;
        }

        public async Task<TransportDetailResponseDto?> UpdateAsync(Guid id, TransportDetailUpdateDto dto)
        {
            var entity = await _unitOfWork.TransportDetails.GetQueryable()
                .Include(td => td.Vehicle)
                .FirstOrDefaultAsync(td => td.Id == id);

            if (entity == null)
            {
                return null;
            }

            if (entity.IsDeleted)
            {
                throw new InvalidOperationException("Không thể cập nhật chi tiết vận chuyển đã bị xóa");
            }

            if (dto.TransportId.HasValue && dto.TransportId.Value != entity.TransportId)
            {
                var transportExists = await _unitOfWork.Transports.AnyAsync(t => t.Id == dto.TransportId.Value && !t.IsDeleted);
                if (!transportExists)
                {
                    throw new KeyNotFoundException("Không tìm thấy chuyến vận chuyển cần cập nhật");
                }

                entity.TransportId = dto.TransportId.Value;
            }

            if (dto.VehicleId.HasValue && dto.VehicleId.Value != entity.VehicleId)
            {
                var vehicle = await _unitOfWork.Vehicles.GetQueryable()
                    .Include(v => v.TransportDetail)
                    .FirstOrDefaultAsync(v => v.Id == dto.VehicleId.Value);

                if (vehicle == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy xe cần cập nhật");
                }

                if (vehicle.TransportDetail != null && vehicle.TransportDetail.Id != entity.Id && !vehicle.TransportDetail.IsDeleted)
                {
                    throw new InvalidOperationException("Xe đã được gán cho chuyến vận chuyển khác");
                }

                entity.VehicleId = dto.VehicleId.Value;
            }

            if (dto.OrderId.HasValue)
            {
                var orderExists = await _unitOfWork.Orders.AnyAsync(o => o.Id == dto.OrderId.Value && !o.IsDeleted);
                if (!orderExists)
                {
                    throw new KeyNotFoundException("Không tìm thấy đơn hàng cần cập nhật");
                }

                entity.OrderId = dto.OrderId.Value;
            }

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.TransportDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<TransportDetailResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.TransportDetails.GetByIdAsync(id);
            if (entity == null)
            {
                return null;
            }

            entity.IsDeleted = isDeleted;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.DeletedDate = isDeleted ? DateTime.UtcNow : null;

            _unitOfWork.TransportDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.TransportDetails.GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            _unitOfWork.TransportDetails.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
