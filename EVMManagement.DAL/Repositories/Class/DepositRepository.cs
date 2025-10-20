using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class DepositRepository : GenericRepository<Deposit>, IDepositRepository
    {
        public DepositRepository(AppDbContext context) : base(context)
        {
        }
    }
}
