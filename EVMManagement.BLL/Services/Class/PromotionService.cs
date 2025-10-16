using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response.Promotion;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto)
        {
            var promotion = new Promotion
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Promotions.AddAsync(promotion);
            await _unitOfWork.SaveChangesAsync();

            return new PromotionResponseDto
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountPercent = promotion.DiscountPercent,
                StartAt = promotion.StartAt,
                EndAt = promotion.EndAt,
                IsActive = promotion.IsActive,
                CreatedDate = promotion.CreatedDate,
                ModifiedDate = promotion.ModifiedDate,
                IsDeleted = promotion.IsDeleted
            };
        }
    }
}

