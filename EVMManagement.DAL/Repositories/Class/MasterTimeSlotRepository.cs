using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class MasterTimeSlotRepository : GenericRepository<MasterTimeSlot>, IMasterTimeSlotRepository
    {
        public MasterTimeSlotRepository(AppDbContext context) : base(context)
        {
        }
    }
}

