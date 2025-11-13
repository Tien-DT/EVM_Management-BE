using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    using EVMManagement.BLL.DTOs.Response.Vehicle;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

    public class VehicleModelService : IVehicleModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleModelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Thông tin mẫu xe không hợp lệ.");
            }

            var normalizedCode = dto.Code?.Trim();
            var normalizedName = dto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                throw new ArgumentException("Mã mẫu xe không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new ArgumentException("Tên mẫu xe không được để trống.");
            }

            dto.Code = normalizedCode;
            dto.Name = normalizedName;

            await EnsureVehicleModelUniquenessAsync(normalizedCode, normalizedName);

            var model = _mapper.Map<VehicleModel>(dto);

            await _unitOfWork.VehicleModels.AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VehicleModelResponseDto>(model);
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetQueryable();

            var totalCount = await query.CountAsync(x => !x.IsDeleted);

            var items = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleModelResponseDto?> GetByIdAsync(Guid id)
        {
            var model = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (model == null) return null;
            return _mapper.Map<VehicleModelResponseDto>(model);
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetByRankingAsync(ranking);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        

        public async Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Thông tin cập nhật không hợp lệ.");
            }

            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null) return null;

            var normalizedCode = dto.Code?.Trim();
            var normalizedName = dto.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                throw new ArgumentException("Mã mẫu xe không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new ArgumentException("Tên mẫu xe không được để trống.");
            }

            await EnsureVehicleModelUniquenessAsync(normalizedCode, normalizedName, id);

            dto.Code = normalizedCode;
            dto.Name = normalizedName;

            _mapper.Map(dto, existing);
            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<VehicleModelResponseDto>(existing);
        }

        public async Task<VehicleModelResponseDto?> SoftDeleteAsync(Guid id)
        {
            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null || existing.IsDeleted) return null;

            existing.IsDeleted = true;
            existing.DeletedDate = DateTime.UtcNow;
            existing.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<VehicleModelResponseDto>(existing);
        }

        private async Task EnsureVehicleModelUniquenessAsync(string code, string name, Guid? excludeId = null)
        {
            var normalizedCodeLower = code.Trim().ToLowerInvariant();
            var normalizedNameLower = name.Trim().ToLowerInvariant();

            var query = _unitOfWork.VehicleModels.GetQueryable()
                .Where(vm => !vm.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(vm => vm.Id != excludeId.Value);
            }

            var conflicts = await query
                .Where(vm =>
                    (vm.Code != null && vm.Code.ToLower() == normalizedCodeLower) ||
                    (vm.Name != null && vm.Name.ToLower() == normalizedNameLower))
                .Select(vm => new { vm.Code, vm.Name })
                .ToListAsync();

            if (conflicts.Any(c => !string.IsNullOrWhiteSpace(c.Code) && string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Mã mẫu xe '{code}' đã tồn tại.");
            }

            if (conflicts.Any(c => !string.IsNullOrWhiteSpace(c.Name) && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Tên mẫu xe '{name}' đã tồn tại.");
            }
        }


        public async Task<PagedResult<VehicleModelResponseDto>> SearchByQueryAsync(string? q, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                GetAllAsync();
            }
            var query = _unitOfWork.VehicleModels.SearchByQueryAsync(q!);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
            
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetByDealerAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var modelsQuery = _unitOfWork.VehicleModels.GetByDealerAsync(dealerId)
                .Where(m => !m.IsDeleted);

            var totalCount = await modelsQuery.CountAsync();

            var items = await modelsQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleModelWithStockResponseDto>> GetAllWithDealerStockAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var warehouseIds = await _unitOfWork.Warehouses.GetQueryable()
                .Where(w => w.DealerId == dealerId && !w.IsDeleted)
                .Select(w => w.Id)
                .ToListAsync();

            var modelsQuery = _unitOfWork.VehicleModels.GetQueryable()
                .Where(m => !m.IsDeleted);

            var totalCount = await modelsQuery.CountAsync();

            var models = await modelsQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var modelIds = models.Select(m => m.Id).ToList();

            var stockCounts = await _unitOfWork.Vehicles.GetQueryable()
                .Where(v => warehouseIds.Contains(v.WarehouseId)
                    && v.Status == VehicleStatus.IN_STOCK
                    && !v.IsDeleted)
                .Join(
                    _unitOfWork.VehicleVariants.GetQueryable(),
                    v => v.VariantId,
                    vv => vv.Id,
                    (v, vv) => new { v, vv.ModelId }
                )
                .Where(x => modelIds.Contains(x.ModelId))
                .GroupBy(x => x.ModelId)
                .Select(g => new { ModelId = g.Key, Count = g.Count() })
                .ToListAsync();

            var responses = models.Select(m => new VehicleModelWithStockResponseDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                LaunchDate = m.LaunchDate,
                Description = m.Description,
                ImageUrl = m.ImageUrl,
                Status = m.Status,
                Ranking = m.Ranking,
                AvailableStock = stockCounts.FirstOrDefault(s => s.ModelId == m.Id)?.Count ?? 0,
                CreatedDate = m.CreatedDate,
                ModifiedDate = m.ModifiedDate
            }).ToList();

            return PagedResult<VehicleModelWithStockResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public IQueryable<VehicleModel> GetQueryableForOData()
        {
            return _unitOfWork.VehicleModels.GetQueryable()
                .Include(vm => vm.VehicleVariants)
                .Where(vm => !vm.IsDeleted);
        }
    }
}
