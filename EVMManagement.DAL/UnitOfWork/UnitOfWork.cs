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
        private bool _transactionRequested = false;
        private IAccountRepository? _accounts;
        private IUserProfileRepository? _userProfiles;
        private IVehicleVariantRepository? _vehicleVariants;
        private IVehicleModelRepository? _vehicleModels;
        private IWarehouseRepository? _warehouses;
        private ICustomerRepository? _customers;
        private IDealerRepository? _dealers;
        private IPromotionRepository? _promotions;
        private IContractRepository? _contracts;
        private IOrderRepository? _orders;
        private IOrderDetailRepository? _orderDetails;
        private IQuotationRepository? _quotations;
        private IQuotationDetailRepository? _quotationDetails;
        
        private IVehicleRepository? _vehicles;

        private IVehicleTimeSlotRepository? _vehicleTimeSlots;
        private IMasterTimeSlotRepository? _masterTimeSlots;
        private IDealerContractRepository? _dealerContracts;
        private ITestDriveBookingRepository? _testDriveBookings;
        private IHandoverRecordRepository? _handoverRecords;
        private IInvoiceRepository? _invoices;
        private IDigitalSignatureRepository? _digitalSignatures;
        private ITransactionRepository? _transactions;
        private IDepositRepository? _deposits;
        private IReportRepository? _reports;
        private IInstallmentPlanRepository? _installmentPlans;
        private ITransportRepository? _transports;
        private ITransportDetailRepository? _transportDetails;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
        public IUserProfileRepository UserProfiles => _userProfiles ??= new UserProfileRepository(_context);
        public IVehicleVariantRepository VehicleVariants => _vehicleVariants ??= new VehicleVariantRepository(_context);
        public IVehicleModelRepository VehicleModels => _vehicleModels ??= new VehicleModelRepository(_context);
        public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_context);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public IDealerRepository Dealers => _dealers ??= new DealerRepository(_context);
        public IPromotionRepository Promotions => _promotions ??= new PromotionRepository(_context);
        public IContractRepository Contracts => _contracts ??= new ContractRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IOrderDetailRepository OrderDetails => _orderDetails ??= new OrderDetailRepository(_context);
        public IQuotationRepository Quotations => _quotations ??= new QuotationRepository(_context);
        public IQuotationDetailRepository QuotationDetails => _quotationDetails ??= new QuotationDetailRepository(_context);
        
        public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);
        public IVehicleTimeSlotRepository VehicleTimeSlots => _vehicleTimeSlots ??= new VehicleTimeSlotRepository(_context);
        public IMasterTimeSlotRepository MasterTimeSlots => _masterTimeSlots ??= new MasterTimeSlotRepository(_context);
        public IDealerContractRepository DealerContracts => _dealerContracts ??= new DealerContractRepository(_context);
        public ITestDriveBookingRepository TestDriveBookings => _testDriveBookings ??= new TestDriveBookingRepository(_context);
        public IHandoverRecordRepository HandoverRecords => _handoverRecords ??= new HandoverRecordRepository(_context);
        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
        public IDigitalSignatureRepository DigitalSignatures => _digitalSignatures ??= new DigitalSignatureRepository(_context);
        public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);
        public IDepositRepository Deposits => _deposits ??= new DepositRepository(_context);
        public IReportRepository Reports => _reports ??= new ReportRepository(_context);
        public IInstallmentPlanRepository InstallmentPlans => _installmentPlans ??= new InstallmentPlanRepository(_context);
        public ITransportRepository Transports => _transports ??= new TransportRepository(_context);
        public ITransportDetailRepository TransportDetails => _transportDetails ??= new TransportDetailRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while saving changes to the database.", ex);
            }
        }

        public Task BeginTransactionAsync()
        {
            _transactionRequested = true;
            return Task.CompletedTask;
        }

        public async Task CommitTransactionAsync()
        {
            if (!_transactionRequested)
            {
                await SaveChangesAsync();
                return;
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                _transaction = await _context.Database.BeginTransactionAsync();
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
                    if (_transaction != null)
                    {
                        await _transaction.RollbackAsync();
                    }
                    throw;
                }
                finally
                {
                    if (_transaction != null)
                    {
                        await _transaction.DisposeAsync();
                        _transaction = null;
                    }
                    _transactionRequested = false;
                }
            });
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            _transactionRequested = false;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}