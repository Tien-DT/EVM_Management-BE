using System;
using System.Threading.Tasks;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }
        IUserProfileRepository UserProfiles { get; }
        IVehicleVariantRepository VehicleVariants { get; }

        // Transaction methods
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
