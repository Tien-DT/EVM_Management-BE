using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }
    }
}
