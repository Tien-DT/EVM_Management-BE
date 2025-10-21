using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Quotation;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IQuotationService
    {
        Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationDto dto);
        Task<PagedResult<QuotationResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, QuotationStatus? status = null);
        Task<IList<QuotationResponseDto>> GetByCustomerIdAsync(Guid customerId);
        Task<QuotationResponseDto?> GetByIdAsync(Guid id);
        Task<QuotationResponseDto?> UpdateAsync(Guid id, UpdateQuotationDto dto);
        Task<QuotationResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
    }
}
