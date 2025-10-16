using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.BLL.DTOs.Request.Quotation;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class QuotationService : IQuotationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuotationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationDto dto)
        {
            var quotation = new Quotation
            {
                Code = dto.Code,
                CustomerId = dto.CustomerId,
                CreatedByUserId = dto.CreatedByUserId,
                Note = dto.Note,
                Status = dto.Status,
                ValidUntil = dto.ValidUntil
            };

            decimal subtotal = 0;
            foreach (var detailDto in dto.QuotationDetails)
            {
                var detail = new QuotationDetail
                {
                    VehicleVariantId = detailDto.VehicleVariantId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice,
                    DiscountPercent = detailDto.DiscountPercent,
                    Note = detailDto.Note
                };

                var lineTotal = detail.UnitPrice * detail.Quantity * (1 - detail.DiscountPercent / 100m);
                subtotal += lineTotal;

                quotation.QuotationDetails.Add(detail);
            }

            quotation.Subtotal = subtotal;
            quotation.Tax = subtotal * 0.1m;
            quotation.Total = subtotal + quotation.Tax;

            await _unitOfWork.Quotations.AddAsync(quotation);
            await _unitOfWork.SaveChangesAsync();

            return (await GetByIdAsync(quotation.Id))!;
        }

        public Task<PagedResult<QuotationResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, QuotationStatus? status = null)
        {
            IQueryable<Quotation> query = _unitOfWork.Quotations.GetQueryable()
                .Include(q => q.Customer)
                .Include(q => q.CreatedByUser)
                .Include(q => q.QuotationDetails)
                    .ThenInclude(qd => qd.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Code.Contains(search) || 
                                         (x.Customer != null && x.Customer.FullName != null && x.Customer.FullName.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            query = query.Where(x => !x.IsDeleted);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QuotationResponseDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer != null ? x.Customer.FullName : null,
                    CustomerPhone = x.Customer != null ? x.Customer.Phone : null,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUser.FullName,
                    Note = x.Note,
                    Subtotal = x.Subtotal,
                    Tax = x.Tax,
                    Total = x.Total,
                    Status = x.Status,
                    ValidUntil = x.ValidUntil,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted,
                    QuotationDetails = x.QuotationDetails.Select(qd => new QuotationDetailResponseDto
                    {
                        Id = qd.Id,
                        QuotationId = qd.QuotationId,
                        VehicleVariantId = qd.VehicleVariantId,
                        VehicleVariantColor = qd.VehicleVariant.Color,
                        VehicleModelName = qd.VehicleVariant.VehicleModel.Name,
                        Quantity = qd.Quantity,
                        UnitPrice = qd.UnitPrice,
                        DiscountPercent = qd.DiscountPercent,
                        LineTotal = qd.UnitPrice * qd.Quantity * (1 - qd.DiscountPercent / 100m),
                        Note = qd.Note
                    }).ToList()
                })
                .ToList();

            return Task.FromResult(PagedResult<QuotationResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<QuotationResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Quotations.GetQueryable()
                .Include(q => q.Customer)
                .Include(q => q.CreatedByUser)
                .Include(q => q.QuotationDetails)
                    .ThenInclude(qd => qd.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null) return null;

            return new QuotationResponseDto
            {
                Id = entity.Id,
                Code = entity.Code,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer?.FullName,
                CustomerPhone = entity.Customer?.Phone,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUserName = entity.CreatedByUser.FullName,
                Note = entity.Note,
                Subtotal = entity.Subtotal,
                Tax = entity.Tax,
                Total = entity.Total,
                Status = entity.Status,
                ValidUntil = entity.ValidUntil,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted,
                QuotationDetails = entity.QuotationDetails.Select(qd => new QuotationDetailResponseDto
                {
                    Id = qd.Id,
                    QuotationId = qd.QuotationId,
                    VehicleVariantId = qd.VehicleVariantId,
                    VehicleVariantColor = qd.VehicleVariant.Color,
                    VehicleModelName = qd.VehicleVariant.VehicleModel.Name,
                    Quantity = qd.Quantity,
                    UnitPrice = qd.UnitPrice,
                    DiscountPercent = qd.DiscountPercent,
                    LineTotal = qd.UnitPrice * qd.Quantity * (1 - qd.DiscountPercent / 100m),
                    Note = qd.Note
                }).ToList()
            };
        }

        public async Task<QuotationResponseDto?> UpdateAsync(Guid id, UpdateQuotationDto dto)
        {
            var entity = await _unitOfWork.Quotations.GetQueryable()
                .Include(q => q.QuotationDetails)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null) return null;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId;
            if (dto.Note != null) entity.Note = dto.Note;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;
            if (dto.ValidUntil.HasValue) entity.ValidUntil = dto.ValidUntil;

            if (dto.QuotationDetails != null && dto.QuotationDetails.Any())
            {
                var existingDetailIds = dto.QuotationDetails
                    .Where(d => d.Id.HasValue)
                    .Select(d => d.Id!.Value)
                    .ToList();

                var detailsToRemove = entity.QuotationDetails
                    .Where(d => !existingDetailIds.Contains(d.Id))
                    .ToList();

                foreach (var detail in detailsToRemove)
                {
                    _unitOfWork.QuotationDetails.Delete(detail);
                }

                decimal subtotal = 0;
                foreach (var detailDto in dto.QuotationDetails)
                {
                    if (detailDto.Id.HasValue)
                    {
                        var existingDetail = entity.QuotationDetails.FirstOrDefault(d => d.Id == detailDto.Id.Value);
                        if (existingDetail != null)
                        {
                            existingDetail.VehicleVariantId = detailDto.VehicleVariantId;
                            existingDetail.Quantity = detailDto.Quantity;
                            existingDetail.UnitPrice = detailDto.UnitPrice;
                            existingDetail.DiscountPercent = detailDto.DiscountPercent;
                            existingDetail.Note = detailDto.Note;
                            existingDetail.ModifiedDate = DateTime.UtcNow;

                            var lineTotal = existingDetail.UnitPrice * existingDetail.Quantity * (1 - existingDetail.DiscountPercent / 100m);
                            subtotal += lineTotal;

                            _unitOfWork.QuotationDetails.Update(existingDetail);
                        }
                    }
                    else
                    {
                        var newDetail = new QuotationDetail
                        {
                            QuotationId = id,
                            VehicleVariantId = detailDto.VehicleVariantId,
                            Quantity = detailDto.Quantity,
                            UnitPrice = detailDto.UnitPrice,
                            DiscountPercent = detailDto.DiscountPercent,
                            Note = detailDto.Note
                        };

                        var lineTotal = newDetail.UnitPrice * newDetail.Quantity * (1 - newDetail.DiscountPercent / 100m);
                        subtotal += lineTotal;

                        await _unitOfWork.QuotationDetails.AddAsync(newDetail);
                    }
                }

                entity.Subtotal = subtotal;
                entity.Tax = subtotal * 0.1m;
                entity.Total = subtotal + entity.Tax;
            }

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Quotations.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<QuotationResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Quotations.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Quotations.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
