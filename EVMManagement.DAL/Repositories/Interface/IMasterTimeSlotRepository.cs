using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IMasterTimeSlotRepository : IGenericRepository<MasterTimeSlot>
    {
        Task<IEnumerable<MasterTimeSlot>> GetByDealerIdAsync(Guid dealerId);
        Task<IEnumerable<MasterTimeSlot>> GetActiveByDealerIdAsync(Guid dealerId);
    }
}

