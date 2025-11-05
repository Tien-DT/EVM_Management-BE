using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.InstallmentPlan;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPlan;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IInstallmentPlanService
    {
        Task<PagedResult<InstallmentPlanResponseDto>> GetByFilterAsync(InstallmentPlanFilterDto filter);
        Task<InstallmentPlanResponseDto?> GetByIdAsync(Guid id);
        Task<InstallmentPlanResponseDto> CreateAsync(InstallmentPlanCreateDto dto);
    }
}
