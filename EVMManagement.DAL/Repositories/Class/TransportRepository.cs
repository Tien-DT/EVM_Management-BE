using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class TransportRepository : GenericRepository<Transport>, ITransportRepository
    {
        public TransportRepository(AppDbContext context) : base(context)
        {
        }
    }
}

