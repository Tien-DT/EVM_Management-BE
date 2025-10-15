using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class DealerRepository : GenericRepository<Dealer>, IDealerRepository
    {
        public DealerRepository(AppDbContext context) : base(context)
        {
        }
    }
}

