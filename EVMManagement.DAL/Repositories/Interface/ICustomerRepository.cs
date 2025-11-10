using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(string phone);
    }
}
