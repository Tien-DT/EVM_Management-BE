using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.HandoverRecord;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
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
            var total = await _unitOfWork.HandoverRecords.CountAsync();

            var entities = await query.OrderByDescending(x => x.CreatedDate)
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
    }
}
