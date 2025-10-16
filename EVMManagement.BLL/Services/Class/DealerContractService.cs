using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Class
{
    public class DealerContractService : IDealerContractService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DealerContractService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DealerContractResponseDto> CreateAsync(DealerContractCreateDto dto)
        {
            var entity = new DealerContract
            {
                DealerId = dto.DealerId,
                ContractCode = dto.ContractCode,
                Terms = dto.Terms,
                Status = dto.Status,
                EffectiveDate = dto.EffectiveDate,
                ExpirationDate = dto.ExpirationDate,
                SignedByDealerUserId = dto.SignedByDealerUserId,
                SignedByEvmUserId = dto.SignedByEvmUserId,
                ContractLink = dto.ContractLink
            };

            await _unitOfWork.DealerContracts.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<PagedResult<DealerContractResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.DealerContracts.GetQueryable();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();
            return PagedResult<DealerContractResponseDto>.Create(responses, total, pageNumber, pageSize);
        }

        public async Task<DealerContractResponseDto?> GetByDealerIdAsync(Guid dealerId)
        {
            var entity = await _unitOfWork.DealerContracts.GetQueryable()
                .Where(d => d.DealerId == dealerId)
                .OrderByDescending(d => d.CreatedDate)
                .FirstOrDefaultAsync();

            if (entity == null) return null;
            return MapToDto(entity);
        }

        public async Task<DealerContractResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.DealerContracts.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        private DealerContractResponseDto MapToDto(DealerContract e)
        {
            return new DealerContractResponseDto
            {
                Id = e.Id,
                DealerId = e.DealerId,
                ContractCode = e.ContractCode,
                Terms = e.Terms,
                Status = e.Status,
                EffectiveDate = e.EffectiveDate,
                ExpirationDate = e.ExpirationDate,
                SignedAt = e.SignedAt,
                SignedByDealerUserId = e.SignedByDealerUserId,
                SignedByEvmUserId = e.SignedByEvmUserId,
                ContractLink = e.ContractLink,
                CreatedDate = e.CreatedDate,
                ModifiedDate = e.ModifiedDate,
                DeletedDate = e.DeletedDate,
                IsDeleted = e.IsDeleted
            };
        }
    }
}
