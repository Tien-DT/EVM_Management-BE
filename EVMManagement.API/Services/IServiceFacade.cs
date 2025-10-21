using EVMManagement.BLL.Services.Interface;

namespace EVMManagement.API.Services
{
    public interface IServiceFacade
    {
        IAuthService AuthService { get; }
        ICustomerService CustomerService { get; }
        IOrderService OrderService { get; }
        IOrderDetailService OrderDetailService { get; }
        IInvoiceService InvoiceService { get; }
        IContractService ContractService { get; }
        IDealerContractService DealerContractService { get; }
        IQuotationService QuotationService { get; }
        IQuotationDetailService QuotationDetailService { get; }
        IDealerService DealerService { get; }
        IPromotionService PromotionService { get; }
        IUserProfileService UserProfileService { get; }
        IVehicleService VehicleService { get; }
        IVehicleModelService VehicleModelService { get; }
        IVehicleVariantService VehicleVariantService { get; }
        IWarehouseService WarehouseService { get; }
        IVehicleTimeSlotService VehicleTimeSlotService { get; }
        IMasterTimeSlotService MasterTimeSlotService { get; }
        IAvailableSlotService AvailableSlotService { get; }
    ITestDriveBookingService TestDriveBookingService { get; }
        IEmailService EmailService { get; }
        IStorageService StorageService { get; }
        IDigitalSignatureService DigitalSignatureService { get; }
        IVnPayService VnPayService { get; }
    }
}
