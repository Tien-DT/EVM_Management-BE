using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Dealer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DealerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Dealer> CreateDealerAsync(CreateDealerDto dto)
        {
            var dealer = new Dealer
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                EstablishedAt = DateTimeHelper.ToUtc(dto.EstablishedAt),
                IsActive = dto.IsActive
            };

            await _unitOfWork.Dealers.AddAsync(dealer);
            await _unitOfWork.SaveChangesAsync();

            return dealer;
        }

        public Task<PagedResult<DealerResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null)
        {
            var query = _unitOfWork.Dealers.GetQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<DealerResponseDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Task.FromResult(PagedResult<DealerResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<DealerResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<DealerResponseDto>(entity);
        }

        public async Task<DealerResponseDto?> UpdateAsync(Guid id, UpdateDealerDto dto)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Name != null) entity.Name = dto.Name;
            if (dto.Address != null) entity.Address = dto.Address;
            if (dto.Phone != null) entity.Phone = dto.Phone;
            if (dto.Email != null) entity.Email = dto.Email;
            if (dto.EstablishedAt.HasValue) entity.EstablishedAt = DateTimeHelper.ToUtc(dto.EstablishedAt);
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Dealers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<DealerResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Dealers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Dealers.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.Dealers.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}

