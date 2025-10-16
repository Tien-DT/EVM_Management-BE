using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.QuotationDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.QuotationDetail;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class QuotationDetailService : IQuotationDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuotationDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<QuotationDetail> CreateQuotationDetailAsync(QuotationDetailCreateDto dto)
        {
            var quotationDetail = new QuotationDetail
            {
                QuotationId = dto.QuotationId,
                VehicleVariantId = dto.VehicleVariantId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                DiscountPercent = dto.DiscountPercent,
                Note = dto.Note
            };

            await _unitOfWork.QuotationDetails.AddAsync(quotationDetail);
            await _unitOfWork.SaveChangesAsync();

            return quotationDetail;
        }

        public async Task<PagedResult<QuotationDetailResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.QuotationDetails.GetQueryable();
            var totalCount = await _unitOfWork.QuotationDetails.CountAsync();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QuotationDetailResponse
                {
                    Id = x.Id,
                    QuotationId = x.QuotationId,
                    VehicleVariantId = x.VehicleVariantId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountPercent = x.DiscountPercent,
                    Note = x.Note,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<QuotationDetailResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<QuotationDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.QuotationDetails.GetByIdAsync(id);
            if (entity == null) return null;

            return new QuotationDetailResponse
            {
                Id = entity.Id,
                QuotationId = entity.QuotationId,
                VehicleVariantId = entity.VehicleVariantId,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                DiscountPercent = entity.DiscountPercent,
                Note = entity.Note,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<QuotationDetailResponse?> UpdateAsync(Guid id, QuotationDetailUpdateDto dto)
        {
            var entity = await _unitOfWork.QuotationDetails.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.QuotationId.HasValue) entity.QuotationId = dto.QuotationId.Value;
            if (dto.VehicleVariantId.HasValue) entity.VehicleVariantId = dto.VehicleVariantId.Value;
            if (dto.Quantity.HasValue) entity.Quantity = dto.Quantity.Value;
            if (dto.UnitPrice.HasValue) entity.UnitPrice = dto.UnitPrice.Value;
            if (dto.DiscountPercent.HasValue) entity.DiscountPercent = dto.DiscountPercent.Value;
            if (dto.Note != null) entity.Note = dto.Note;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.QuotationDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<QuotationDetailResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.QuotationDetails.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.QuotationDetails.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.QuotationDetails.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.QuotationDetails.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
