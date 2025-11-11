using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;
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
            if (dto.StartAt.HasValue && dto.EndAt.HasValue && dto.StartAt > dto.EndAt)
            {
                throw new ArgumentException("Thoi gian bat dau phai nho hon hoac bang thoi gian ket thuc.");
            }

            var variantIds = dto.VariantIds?
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (variantIds.Count > 0)
            {
                var existingVariantIds = await _unitOfWork.VehicleVariants.GetQueryable()
                    .Where(v => variantIds.Contains(v.Id) && !v.IsDeleted)
                    .Select(v => v.Id)
                    .ToListAsync();

                var missingVariantIds = variantIds.Except(existingVariantIds).ToList();
                if (missingVariantIds.Any())
                {
                    throw new ArgumentException("Khong tim thay cac phien ban xe: " + string.Join(", ", missingVariantIds));
                }
            }

            var promotion = _mapper.Map<Promotion>(dto);

            await _unitOfWork.Promotions.AddAsync(promotion);

            if (variantIds.Count > 0)
            {
                var vehiclePromotions = variantIds.Select(id => new VehiclePromotion
                {
                    VariantId = id,
                    PromotionId = promotion.Id
                }).ToList();

                await _unitOfWork.VehiclePromotions.AddRangeAsync(vehiclePromotions);
                promotion.VehiclePromotions = vehiclePromotions;
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionResponseDto>(promotion);
        }

        public async Task<PagedResult<PromotionResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Promotions.GetQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<PromotionResponseDto>>(items);

            return PagedResult<PromotionResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PromotionResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<PromotionResponseDto>(entity);
        }

        public async Task<PagedResult<PromotionResponseDto>> SearchAsync(string? query, int pageNumber = 1, int pageSize = 10)
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

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<PromotionResponseDto>>(items);

            return PagedResult<PromotionResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PromotionResponseDto?> UpdateAsync(Guid id, PromotionUpdateDto dto)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Promotions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionResponseDto>(entity);
        }

        public async Task<PromotionResponseDto?> UpdateIsActiveAsync(Guid id, bool isActive)
        {
            var entity = await _unitOfWork.Promotions.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsActive = isActive;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Promotions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PromotionResponseDto>(entity);
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

            return _mapper.Map<PromotionResponseDto>(entity);
        }

        public async Task<PagedResult<VehiclePromotionResponseDto>> GetVehiclePromotionsAsync(Guid? variantId, Guid? promotionId, int pageNumber = 1, int pageSize = 10)
        {
            if (!variantId.HasValue && !promotionId.HasValue)
            {
                throw new ArgumentException("Can cung cap VariantId hoac PromotionId.");
            }

            var now = DateTime.UtcNow;
            var query = _unitOfWork.VehiclePromotions.GetWithRelations()
                .Where(vp => vp.Promotion.IsActive && !vp.Promotion.IsDeleted)
                .Where(vp =>
                    (!vp.Promotion.StartAt.HasValue || vp.Promotion.StartAt <= now) &&
                    (!vp.Promotion.EndAt.HasValue || vp.Promotion.EndAt >= now));

            if (variantId.HasValue)
            {
                query = query.Where(vp => vp.VariantId == variantId.Value);
            }

            if (promotionId.HasValue)
            {
                query = query.Where(vp => vp.PromotionId == promotionId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(vp => vp.Promotion.StartAt ?? vp.Promotion.CreatedDate)
                .ThenBy(vp => vp.VehicleVariant.Color)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehiclePromotionResponseDto>>(items);

            return PagedResult<VehiclePromotionResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
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

