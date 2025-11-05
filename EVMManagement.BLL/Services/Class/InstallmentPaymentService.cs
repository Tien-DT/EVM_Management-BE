using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.InstallmentPayment;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPayment;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class InstallmentPaymentService : IInstallmentPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InstallmentPaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<InstallmentPaymentResponseDto>> GetByFilterAsync(InstallmentPaymentFilterDto filter)
        {
            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                throw new ArgumentException("Trang và kích thước trang phải lớn hơn 0.");
            }

            var query = _unitOfWork.InstallmentPayments.GetQueryable()
                .Where(x => !x.IsDeleted);

            if (filter.Id.HasValue)
            {
                query = query.Where(x => x.Id == filter.Id.Value);
            }

            if (filter.PlanId.HasValue)
            {
                query = query.Where(x => x.PlanId == filter.PlanId.Value);
            }

            if (filter.OrderId.HasValue)
            {
                query = query.Where(x => x.InstallmentPlan != null && x.InstallmentPlan.OrderId == filter.OrderId.Value);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(x => x.InstallmentPlan != null && x.InstallmentPlan.Order != null && x.InstallmentPlan.Order.CustomerId == filter.CustomerId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                var fromDate = DateTimeHelper.ToUtc(filter.DueDateFrom.Value);
                query = query.Where(x => x.DueDate >= fromDate);
            }

            if (filter.DueDateTo.HasValue)
            {
                var toDate = DateTimeHelper.ToUtc(filter.DueDateTo.Value);
                query = query.Where(x => x.DueDate <= toDate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.DueDate)
                .ThenBy(x => x.InstallmentNumber)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<InstallmentPaymentResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<InstallmentPaymentResponseDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<InstallmentPaymentResponseDto?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.InstallmentPayments.GetQueryable()
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<InstallmentPaymentResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<InstallmentPaymentResponseDto> CreateAsync(InstallmentPaymentCreateDto dto)
        {
            var plan = await _unitOfWork.InstallmentPlans.GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == dto.PlanId && !x.IsDeleted);

            if (plan == null)
            {
                throw new KeyNotFoundException("Không tìm thấy kế hoạch trả góp tương ứng.");
            }

            var duplicated = await _unitOfWork.InstallmentPayments.GetQueryable()
                .AnyAsync(x => x.PlanId == dto.PlanId && x.InstallmentNumber == dto.InstallmentNumber && !x.IsDeleted);

            if (duplicated)
            {
                throw new InvalidOperationException("Kỳ thanh toán đã tồn tại trong kế hoạch này.");
            }

            var payment = new InstallmentPayment
            {
                PlanId = dto.PlanId,
                InstallmentNumber = dto.InstallmentNumber,
                AmountDue = dto.AmountDue,
                DueDate = DateTimeHelper.ToUtc(dto.DueDate),
                Status = dto.Status
            };

            await _unitOfWork.InstallmentPayments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return (await _unitOfWork.InstallmentPayments.GetQueryable()
                .Where(x => x.Id == payment.Id)
                .ProjectTo<InstallmentPaymentResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync())!;
        }

        public async Task<InstallmentPaymentResponseDto?> UpdateAsync(Guid id, InstallmentPaymentUpdateDto dto)
        {
            var payment = await _unitOfWork.InstallmentPayments.GetQueryable()
                .Include(x => x.InstallmentPlan)
                .ThenInclude(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (payment == null)
            {
                return null;
            }

            if (dto.PlanId.HasValue && dto.PlanId.Value != payment.PlanId)
            {
                var plan = await _unitOfWork.InstallmentPlans.GetQueryable()
                    .FirstOrDefaultAsync(x => x.Id == dto.PlanId.Value && !x.IsDeleted);

                if (plan == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy kế hoạch trả góp mới.");
                }

                var duplicated = await _unitOfWork.InstallmentPayments.GetQueryable()
                    .AnyAsync(x => x.PlanId == dto.PlanId.Value && x.InstallmentNumber == (dto.InstallmentNumber ?? payment.InstallmentNumber) && x.Id != payment.Id && !x.IsDeleted);

                if (duplicated)
                {
                    throw new InvalidOperationException("Kỳ thanh toán đã tồn tại trong kế hoạch mới.");
                }

                payment.PlanId = dto.PlanId.Value;
            }
            else if (dto.InstallmentNumber.HasValue)
            {
                var duplicated = await _unitOfWork.InstallmentPayments.GetQueryable()
                    .AnyAsync(x => x.PlanId == payment.PlanId && x.InstallmentNumber == dto.InstallmentNumber.Value && x.Id != payment.Id && !x.IsDeleted);

                if (duplicated)
                {
                    throw new InvalidOperationException("Kỳ thanh toán đã tồn tại trong kế hoạch này.");
                }

                payment.InstallmentNumber = dto.InstallmentNumber.Value;
            }

            if (dto.AmountDue.HasValue)
            {
                payment.AmountDue = dto.AmountDue.Value;
            }

            if (dto.DueDate.HasValue)
            {
                payment.DueDate = DateTimeHelper.ToUtc(dto.DueDate.Value);
            }

            if (dto.Status.HasValue)
            {
                payment.Status = dto.Status.Value;
            }

            payment.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.InstallmentPayments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.InstallmentPayments.GetQueryable()
                .Where(x => x.Id == payment.Id)
                .ProjectTo<InstallmentPaymentResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var payment = await _unitOfWork.InstallmentPayments.GetByIdAsync(id);
            if (payment == null)
            {
                return false;
            }

            _unitOfWork.InstallmentPayments.Delete(payment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
