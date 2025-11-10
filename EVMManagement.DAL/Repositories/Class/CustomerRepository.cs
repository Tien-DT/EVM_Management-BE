using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.DAL.Repositories.Class
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Phone == phone && !c.IsDeleted);
        }
    }
}
