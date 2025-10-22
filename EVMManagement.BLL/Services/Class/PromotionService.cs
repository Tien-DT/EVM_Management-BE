using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto)
        {
            var promotion = new Promotion
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                StartAt = DateTimeHelper.ToUtc(dto.StartAt),
                EndAt = DateTimeHelper.ToUtc(dto.EndAt),
                IsActive = dto.IsActive
            };

            await _unitOfWork.Promotions.AddAsync(promotion);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionResponseDto>(promotion);
        }

        public async Task<PagedResult<PromotionResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Promotions.GetQueryable();
            var totalCount = await _unitOfWork.Promotions.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<PromotionResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<PromotionResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PromotionResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<PromotionResponseDto>(entity);
        }

        public Task<PagedResult<PromotionResponseDto>> SearchAsync(string? query, int pageNumber = 1, int pageSize = 10)
        {
            var queryable = _unitOfWork.Promotions.GetQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                queryable = queryable.Where(x =>
                    (x.Code != null && x.Code.ToLower().Contains(lowerQuery)) ||
                    (x.Name != null && x.Name.ToLower().Contains(lowerQuery)) ||
                    (x.Description != null && x.Description.ToLower().Contains(lowerQuery))
                );
            }

            var totalCount = queryable.Count();

            var items = queryable
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<PromotionResponseDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Task.FromResult(PagedResult<PromotionResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<PromotionResponseDto?> UpdateAsync(Guid id, PromotionUpdateDto dto)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.DiscountPercent.HasValue) entity.DiscountPercent = dto.DiscountPercent;
            if (dto.StartAt.HasValue) entity.StartAt = DateTimeHelper.ToUtc(dto.StartAt);
            if (dto.EndAt.HasValue) entity.EndAt = DateTimeHelper.ToUtc(dto.EndAt);
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Promotions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<PromotionResponseDto?> UpdateIsActiveAsync(Guid id, bool isActive)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsActive = isActive;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Promotions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<PromotionResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Promotions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Promotions.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}

