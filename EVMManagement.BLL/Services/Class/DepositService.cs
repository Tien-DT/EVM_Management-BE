using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Deposit;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Deposit;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class DepositService : IDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepositService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DepositResponse> CreateAsync(DepositCreateDto dto)
        {
            var entity = _mapper.Map<Deposit>(dto);

            await _unitOfWork.Deposits.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepositResponse>(entity);
        }

        public async Task<PagedResult<DepositResponse>> GetAsync(Guid? orderId, Guid? receivedByUserId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Deposits.GetQueryable().Where(x => !x.IsDeleted);

            if (orderId.HasValue)
            {
                query = query.Where(x => x.OrderId == orderId.Value);
            }

            if (receivedByUserId.HasValue)
            {
                query = query.Where(x => x.ReceivedByUserId == receivedByUserId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<DepositResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<DepositResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<DepositResponse?> GetByIdAsync(Guid id)
        {
            var query = _unitOfWork.Deposits.GetQueryable()
                .Where(x => x.Id == id && !x.IsDeleted);

            return await query
                .ProjectTo<DepositResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<DepositResponse?> UpdateAsync(Guid id, DepositUpdateDto dto)
        {
            var entity = await _unitOfWork.Deposits.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return null;
            }

            if (dto.OrderId.HasValue)
            {
                entity.OrderId = dto.OrderId.Value;
            }

            if (dto.Amount.HasValue)
            {
                entity.Amount = dto.Amount.Value;
            }

            if (dto.Method.HasValue)
            {
                entity.Method = dto.Method.Value;
            }

            if (dto.Status.HasValue)
            {
                entity.Status = dto.Status.Value;
            }

            if (dto.ReceivedByUserId.HasValue)
            {
                entity.ReceivedByUserId = dto.ReceivedByUserId.Value;
            }

            if (dto.Note != null)
            {
                entity.Note = dto.Note;
            }

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Deposits.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Deposits.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Deposits.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<DepositResponse> CreatePreOrderDepositAsync(PreOrderDepositRequestDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {dto.OrderId} not found");
            }

            // tính 10% tiền đặt cọc
            var depositAmount = (order.FinalAmount ?? 0) * 0.10m;

            var combinedNote = string.Empty;
            if (!string.IsNullOrWhiteSpace(dto.BillImageUrl))
            {
                combinedNote = $"Bill Image: {dto.BillImageUrl}";
                if (!string.IsNullOrWhiteSpace(dto.Note))
                {
                    combinedNote += $"\n{dto.Note}";
                }
            }
            else
            {
                combinedNote = dto.Note ?? string.Empty;
            }

            var deposit = new Deposit
            {
                OrderId = dto.OrderId,
                Amount = depositAmount,
                Method = dto.Method,
                Status = DepositStatus.PAID,
                Note = combinedNote
            };

            await _unitOfWork.Deposits.AddAsync(deposit);

            order.Status = OrderStatus.IN_PROGRESS;
            order.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepositResponse>(deposit);
        }
    }
}
