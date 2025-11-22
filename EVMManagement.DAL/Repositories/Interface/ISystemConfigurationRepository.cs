using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface ISystemConfigurationRepository : IGenericRepository<SystemConfiguration>
    {
        Task<SystemConfiguration?> GetByKeyAsync(string key);
        Task<decimal> GetTaxRateAsync();
    }
}
