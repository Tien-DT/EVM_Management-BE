using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Contract> CreateContractAsync(ContractCreateDto dto)
        {
            var contract = new Contract
            {
                Code = dto.Code,
                OrderId = dto.OrderId,
                CustomerId = dto.CustomerId,
                DealerId = dto.DealerId,
                CreatedByUserId = dto.CreatedByUserId,
                SignedByUserId = dto.SignedByUserId,
                Terms = dto.Terms,
                Status = dto.Status,
                ContractType = dto.ContractType,
                SignedAt = DateTimeHelper.ToUtc(dto.SignedAt),
                ContractLink = dto.ContractLink
            };

            await _unitOfWork.Contracts.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            return contract;
        }

        public async Task<PagedResult<ContractDetailResponse>> GetAllAsync(Guid? orderId, Guid? customerId, Guid? dealerId, Guid? createdByUserId, Guid? signedByUserId, ContractStatus? status, ContractType? contractType, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Contracts.GetContractsWithDetails(orderId, customerId, dealerId, createdByUserId, signedByUserId, status, contractType);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ContractDetailResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<ContractDetailResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<ContractResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Contracts.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;

            return _mapper.Map<ContractResponse>(entity);
        }

        public async Task<ContractResponse?> UpdateAsync(Guid id, ContractUpdateDto dto)
        {
            var entity = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.OrderId.HasValue) entity.OrderId = dto.OrderId.Value;
            if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId.Value;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId.Value;
            if (dto.CreatedByUserId.HasValue) entity.CreatedByUserId = dto.CreatedByUserId.Value;
            if (dto.SignedByUserId.HasValue) entity.SignedByUserId = dto.SignedByUserId.Value;
            if (dto.Terms != null) entity.Terms = dto.Terms;

            bool hasSignedContract = false;

            if (dto.ContractLink != null)
            {
                var trimmedLink = string.IsNullOrWhiteSpace(dto.ContractLink)
                    ? null
                    : dto.ContractLink.Trim();
                entity.ContractLink = trimmedLink;

                if (!string.IsNullOrWhiteSpace(trimmedLink))
                {
                    hasSignedContract = true;
                }
            }

            if (dto.Status.HasValue)
            {
                entity.Status = dto.Status.Value;
            }
            else if (hasSignedContract)
            {
                entity.Status = ContractStatus.ACTIVE;
            }

            if (dto.ContractType.HasValue)
            {
                entity.ContractType = dto.ContractType.Value;
            }

            if (dto.SignedAt.HasValue)
            {
                entity.SignedAt = DateTimeHelper.ToUtc(dto.SignedAt);
            }
            else if (hasSignedContract && !entity.SignedAt.HasValue)
            {
                entity.SignedAt = DateTime.UtcNow;
            }

            entity.ModifiedDate = DateTime.UtcNow;

            if (hasSignedContract)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(entity.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.IN_PROGRESS;
                    order.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);
                }
            }

            _unitOfWork.Contracts.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<ContractResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Contracts.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Contracts.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ContractDetailResponse>> GetByDealerIdAsync(Guid dealerId, ContractStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Contracts.GetContractsByDealerId(dealerId, status);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ContractDetailResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<ContractDetailResponse>.Create(items, totalCount, pageNumber, pageSize);
        }
    }
}
