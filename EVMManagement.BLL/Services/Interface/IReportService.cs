using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Report;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Report;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IReportService
    {
        Task<ReportResponse> CreateAsync(ReportCreateDto dto);
        Task<PagedResult<ReportResponse>> GetAsync(Guid? dealerId, Guid? accountId, int pageNumber, int pageSize);
        Task<ReportResponse?> GetByIdAsync(Guid id);
        Task<ReportResponse?> UpdateAsync(Guid id, ReportUpdateDto dto);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
