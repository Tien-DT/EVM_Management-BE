using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class SystemConfigurationRepository : GenericRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SystemConfiguration?> GetByKeyAsync(string key)
        {
            return await _context.SystemConfigurations
                .Where(sc => sc.Key == key && sc.IsActive && !sc.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTaxRateAsync()
        {
            var config = await GetByKeyAsync("TaxRate");
            if (config != null && decimal.TryParse(config.Value, out var taxRate))
            {
                return taxRate;
            }
            return 0.10m;
        }
    }
}
