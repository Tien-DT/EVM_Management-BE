using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.QuotationDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.QuotationDetail;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IQuotationDetailService
    {
        Task<QuotationDetail> CreateQuotationDetailAsync(QuotationDetailCreateDto dto);
        Task<PagedResult<QuotationDetailResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<QuotationDetailResponse?> GetByIdAsync(Guid id);
        Task<IList<QuotationDetailWithOrderResponse>> GetByQuotationIdAsync(Guid quotationId);
        Task<QuotationDetailResponse?> UpdateAsync(Guid id, QuotationDetailUpdateDto dto);
        Task<QuotationDetailResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
