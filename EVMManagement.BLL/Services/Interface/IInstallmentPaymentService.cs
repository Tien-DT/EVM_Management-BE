using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.InstallmentPayment;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPayment;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IInstallmentPaymentService
    {
        Task<PagedResult<InstallmentPaymentResponseDto>> GetByFilterAsync(InstallmentPaymentFilterDto filter);
        Task<InstallmentPaymentResponseDto?> GetByIdAsync(Guid id);
        Task<InstallmentPaymentResponseDto> CreateAsync(InstallmentPaymentCreateDto dto);
        Task<InstallmentPaymentResponseDto?> UpdateAsync(Guid id, InstallmentPaymentUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
