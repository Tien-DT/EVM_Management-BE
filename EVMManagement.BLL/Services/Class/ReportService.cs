using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.Report;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Report;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReportResponse> CreateAsync(ReportCreateDto dto)
        {
            var entity = _mapper.Map<Report>(dto);

            await _unitOfWork.Reports.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReportResponse>(entity);
        }

        public async Task<PagedResult<ReportResponse>> GetAsync(Guid? dealerId, Guid? accountId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Reports.GetQueryable().Where(x => !x.IsDeleted);

            if (accountId.HasValue)
            {
                query = query.Where(x => x.AccountId == accountId.Value);
            }

            if (dealerId.HasValue)
            {
                query = query.Where(x => x.DealerId == dealerId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<ReportResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<ReportResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<ReportResponse?> GetByIdAsync(Guid id)
        {
            var query = _unitOfWork.Reports.GetQueryable()
                .Where(x => x.Id == id && !x.IsDeleted);

            return await query
                .ProjectTo<ReportResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<ReportResponse?> UpdateAsync(Guid id, ReportUpdateDto dto)
        {
            var entity = await _unitOfWork.Reports.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return null;
            }

            if (dto.AccountId.HasValue)
            {
                entity.AccountId = dto.AccountId.Value;
            }

            if (dto.DealerId.HasValue)
            {
                entity.DealerId = dto.DealerId;
            }

            if (dto.Type != null)
            {
                entity.Type = dto.Type;
            }

            if (dto.Title != null)
            {
                entity.Title = dto.Title;
            }

            if (dto.Content != null)
            {
                entity.Content = dto.Content;
            }

            if (dto.OrderId.HasValue)
            {
                entity.OrderId = dto.OrderId;
            }

            if (dto.TransportId.HasValue)
            {
                entity.TransportId = dto.TransportId;
            }

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Reports.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Reports.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return false;
            }

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Reports.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
