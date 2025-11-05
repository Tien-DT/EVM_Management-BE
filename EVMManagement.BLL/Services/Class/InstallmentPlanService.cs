using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.InstallmentPlan;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPlan;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class InstallmentPlanService : IInstallmentPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InstallmentPlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<InstallmentPlanResponseDto>> GetByFilterAsync(InstallmentPlanFilterDto filter)
        {
            if (!filter.Id.HasValue && !filter.OrderId.HasValue && !filter.CustomerId.HasValue)
            {
                throw new ArgumentException("Vui lòng cung cấp Id, OrderId hoặc CustomerId để lọc kế hoạch trả góp.");
            }

            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                throw new ArgumentException("Trang và kích thước trang phải lớn hơn 0.");
            }

            var query = _unitOfWork.InstallmentPlans.GetQueryable()
                .Where(x => !x.IsDeleted);

            if (filter.Id.HasValue)
            {
                query = query.Where(x => x.Id == filter.Id.Value);
            }

            if (filter.OrderId.HasValue)
            {
                query = query.Where(x => x.OrderId == filter.OrderId.Value);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(x => x.Order != null && x.Order.CustomerId == filter.CustomerId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ProjectTo<InstallmentPlanResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<InstallmentPlanResponseDto>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<InstallmentPlanResponseDto?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.InstallmentPlans.GetQueryable()
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<InstallmentPlanResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<InstallmentPlanResponseDto> CreateAsync(InstallmentPlanCreateDto dto)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == dto.OrderId && !x.IsDeleted);

            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng để tạo kế hoạch trả góp.");
            }

            var existingPlan = await _unitOfWork.InstallmentPlans.AnyAsync(x => x.OrderId == dto.OrderId && !x.IsDeleted);
            if (existingPlan)
            {
                throw new InvalidOperationException("Đơn hàng đã có kế hoạch trả góp.");
            }

            var startDate = DateTimeHelper.ToUtc(dto.StartDate);

            var plan = new InstallmentPlan
            {
                OrderId = dto.OrderId,
                Provider = dto.Provider,
                PrincipalAmount = dto.PrincipalAmount,
                InterestRate = dto.InterestRate,
                NumberOfInstallments = dto.NumberOfInstallments,
                Status = dto.Status,
                StartDate = startDate
            };

            var totalAmount = dto.PrincipalAmount * (1 + dto.InterestRate / 100m);
            var payments = BuildInstallmentPayments(plan.Id, plan.NumberOfInstallments, totalAmount, startDate);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.InstallmentPlans.AddAsync(plan);
                if (payments.Count > 0)
                {
                    await _unitOfWork.InstallmentPayments.AddRangeAsync(payments);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Không thể tạo kế hoạch trả góp, vui lòng thử lại.", ex);
            }

            return (await _unitOfWork.InstallmentPlans.GetQueryable()
                .Where(x => x.Id == plan.Id)
                .ProjectTo<InstallmentPlanResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync())!;
        }

        private static List<InstallmentPayment> BuildInstallmentPayments(Guid planId, int count, decimal totalAmount, DateTime startDate)
        {
            var payments = new List<InstallmentPayment>();

            if (count <= 0)
            {
                return payments;
            }

            var regularAmount = Math.Round(totalAmount / count, 2, MidpointRounding.AwayFromZero);
            decimal allocated = 0;

            for (var i = 1; i <= count; i++)
            {
                decimal amount;
                if (i == count)
                {
                    amount = Math.Round(totalAmount - allocated, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    amount = regularAmount;
                    allocated += amount;
                }

                var dueDate = startDate.AddMonths(i - 1);

                payments.Add(new InstallmentPayment
                {
                    PlanId = planId,
                    InstallmentNumber = i,
                    AmountDue = amount,
                    DueDate = dueDate,
                    Status = InstallmentPaymentStatus.PENDING
                });
            }

            return payments;
        }
    }
}
