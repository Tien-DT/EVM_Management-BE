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
        IVehicleModelRepository VehicleModels { get; }
        ICustomerRepository Customers { get; }
        IWarehouseRepository Warehouses { get; }
        IDealerRepository Dealers { get; }
        IPromotionRepository Promotions { get; }
        IContractRepository Contracts { get; }
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }
        IQuotationRepository Quotations { get; }
        IQuotationDetailRepository QuotationDetails { get; }
       
        IVehicleRepository Vehicles { get; }
        IVehicleTimeSlotRepository VehicleTimeSlots { get; }
        IMasterTimeSlotRepository MasterTimeSlots { get; }
        IAvailableSlotRepository AvailableSlots { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
