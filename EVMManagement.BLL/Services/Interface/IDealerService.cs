using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Dealer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IDealerService
    {
        Task<Dealer> CreateDealerAsync(CreateDealerDto dto);
        Task<PagedResult<DealerResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null);
        Task<DealerResponseDto?> GetByIdAsync(Guid id);
        Task<DealerResponseDto?> UpdateAsync(Guid id, UpdateDealerDto dto);
        Task<DealerResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
        IQueryable<Dealer> GetQueryableForOData();
    }
}

