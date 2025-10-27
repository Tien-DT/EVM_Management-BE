using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class TransportDetailRepository : GenericRepository<TransportDetail>, ITransportDetailRepository
    {
        public TransportDetailRepository(AppDbContext context) : base(context)
        {
        }
    }
}

