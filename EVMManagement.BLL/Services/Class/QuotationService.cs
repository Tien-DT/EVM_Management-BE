using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using EVMManagement.BLL.DTOs.Request.Quotation;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class QuotationService : IQuotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuotationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationDto dto)
        {
            // Find UserProfile by AccountId (in case dto.CreatedByUserId is AccountId)
            var userProfile = await _unitOfWork.UserProfiles.GetQueryable()
                .FirstOrDefaultAsync(up => up.Id == dto.CreatedByUserId || up.AccountId == dto.CreatedByUserId);

            if (userProfile == null)
            {
                throw new Exception($"User profile not found for ID: {dto.CreatedByUserId}");
            }

            var quotation = new Quotation
            {
                Code = dto.Code,
                CustomerId = dto.CustomerId,
                CreatedByUserId = userProfile.Id, // Use UserProfile.Id
                Note = dto.Note,
                Status = dto.Status,
                ValidUntil = DateTimeHelper.ToUtc(dto.ValidUntil)
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

            // If quotation is created for an existing order, update order status and link quotation
            if (dto.OrderId.HasValue)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId.Value);
                if (order != null)
                {
                    order.QuotationId = quotation.Id;
                    order.Status = OrderStatus.QUOTATION_RECEIVED;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return (await GetByIdAsync(quotation.Id))!;
        }

        public async Task<PagedResult<QuotationResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, QuotationStatus? status = null)
        {
            var query = _unitOfWork.Quotations.GetQueryable()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Code.Contains(search) ||
                                         (x.Customer != null && x.Customer.FullName != null && x.Customer.FullName.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<QuotationResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<QuotationResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<IList<QuotationResponseDto>> GetByCustomerIdAsync(Guid customerId)
        {
            var query = _unitOfWork.Quotations.GetByCustomerId(customerId);

            return await query
                .OrderByDescending(x => x.CreatedDate)
                .ProjectTo<QuotationResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<QuotationResponseDto?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Quotations.GetQueryable()
                .Where(q => q.Id == id)
                .ProjectTo<QuotationResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<QuotationResponseDto?> ConfirmQuotationAsync(Guid id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var quotation = await _unitOfWork.Quotations.GetQueryable()
                    .Include(q => q.QuotationDetails)
                    .Include(q => q.Order)
                    .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

                if (quotation == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

                var subtotal = quotation.QuotationDetails.Sum(detail =>
                {
                    var discountRate = detail.DiscountPercent / 100m;
                    var effectiveRate = 1 - discountRate;
                    if (effectiveRate < 0)
                    {
                        effectiveRate = 0;
                    }

                    return detail.UnitPrice * detail.Quantity * effectiveRate;
                });

                var tax = subtotal * 0.1m;
                var total = subtotal + tax;

                quotation.Subtotal = subtotal;
                quotation.Tax = tax;
                quotation.Total = total;
                quotation.Status = QuotationStatus.ACCEPTED;
                quotation.ModifiedDate = DateTime.UtcNow;

                var order = quotation.Order;
                if (order == null)
                {
                    order = await _unitOfWork.Orders.GetQueryable()
                        .FirstOrDefaultAsync(o => o.QuotationId == quotation.Id && !o.IsDeleted);
                }

                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Order not found for the quotation.");
                }

                var discount = order.DiscountAmount;
                if (discount.HasValue && discount.Value > total)
                {
                    discount = total;
                    order.DiscountAmount = discount;
                }

                order.TotalAmount = total;
                if (discount.HasValue)
                {
                    var final = total - discount.Value;
                    if (final < 0)
                    {
                        final = 0;
                    }

                    order.FinalAmount = final;
                }
                else
                {
                    order.FinalAmount = total;
                }
                order.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Quotations.Update(quotation);
                _unitOfWork.Orders.Update(order);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return await GetByIdAsync(id);
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
            if (dto.ValidUntil.HasValue) entity.ValidUntil = DateTimeHelper.ToUtc(dto.ValidUntil);

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
