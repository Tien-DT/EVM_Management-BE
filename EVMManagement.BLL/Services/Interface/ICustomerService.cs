using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(CustomerCreateDto dto);
        Task<PagedResult<CustomerResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<CustomerResponse?> GetByIdAsync(Guid id);
        Task<CustomerResponse?> UpdateAsync(Guid id, CustomerUpdateDto dto);
        Task<CustomerResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
