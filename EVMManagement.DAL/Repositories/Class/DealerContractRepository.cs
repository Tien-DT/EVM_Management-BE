using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class DealerContractRepository : GenericRepository<DealerContract>, IDealerContractRepository
    {
        public DealerContractRepository(AppDbContext context) : base(context)
        {
        }
    }
}
