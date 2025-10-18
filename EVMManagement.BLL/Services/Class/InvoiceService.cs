using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Invoice;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Invoice;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Invoice> CreateInvoiceAsync(InvoiceCreateDto dto)
        {
            var invoice = new Invoice
            {
                OrderId = dto.OrderId,
                InvoiceCode = dto.InvoiceCode,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return invoice;
        }

        public async Task<PagedResult<InvoiceResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Invoices.GetQueryable();
            var totalCount = await _unitOfWork.Invoices.CountAsync();

            var items = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InvoiceResponse
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    InvoiceCode = x.InvoiceCode,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<InvoiceResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<InvoiceResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (entity == null) return null;

            return new InvoiceResponse
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                InvoiceCode = entity.InvoiceCode,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<InvoiceResponse?> UpdateAsync(Guid id, InvoiceUpdateDto dto)
        {
            var entity = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.OrderId.HasValue) entity.OrderId = dto.OrderId.Value;
            if (dto.InvoiceCode != null) entity.InvoiceCode = dto.InvoiceCode;
            if (dto.TotalAmount.HasValue) entity.TotalAmount = dto.TotalAmount.Value;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Invoices.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<InvoiceResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Invoices.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Invoices.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
