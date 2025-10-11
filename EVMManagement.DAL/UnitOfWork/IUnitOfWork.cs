using System;
using System.Threading.Tasks;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {

        // Transaction methods
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
