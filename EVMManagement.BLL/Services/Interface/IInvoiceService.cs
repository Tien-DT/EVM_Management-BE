using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Invoice;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Invoice;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(InvoiceCreateDto dto);
        Task<PagedResult<InvoiceResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<InvoiceResponse?> GetByIdAsync(Guid id);
        Task<InvoiceResponse?> UpdateAsync(Guid id, InvoiceUpdateDto dto);
        Task<InvoiceResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
