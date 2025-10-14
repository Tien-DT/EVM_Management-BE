using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Repositories.Class;

namespace EVMManagement.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private IAccountRepository? _accounts;
        private IUserProfileRepository? _userProfiles;
        private IVehicleVariantRepository? _vehicleVariants;
        private IVehicleModelRepository? _vehicleModels;
    private IWarehouseRepository? _warehouses;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
        public IUserProfileRepository UserProfiles => _userProfiles ??= new UserProfileRepository(_context);
        public IVehicleVariantRepository VehicleVariants => _vehicleVariants ??= new VehicleVariantRepository(_context);
        public IVehicleModelRepository VehicleModels => _vehicleModels ??= new VehicleModelRepository(_context);
        public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the error (uncomment ex variable name and write a log)
                throw new Exception("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
