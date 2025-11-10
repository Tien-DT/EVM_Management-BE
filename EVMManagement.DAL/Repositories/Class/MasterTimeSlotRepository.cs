using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<MasterTimeSlot>> GetByDealerIdAsync(Guid dealerId)
        {
            return await _context.MasterTimeSlots
                .Where(m => m.DealerId == dealerId && !m.IsDeleted)
                .OrderBy(m => m.StartOffsetMinutes)
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterTimeSlot>> GetActiveByDealerIdAsync(Guid dealerId)
        {
            return await _context.MasterTimeSlots
                .Where(m => m.DealerId == dealerId && m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.StartOffsetMinutes)
                .ToListAsync();
        }
    }
}

