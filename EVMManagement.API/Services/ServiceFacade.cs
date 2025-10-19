using EVMManagement.BLL.Services.Interface;

namespace EVMManagement.API.Services
{
    public class ServiceFacade : IServiceFacade
    {
        public IAuthService AuthService { get; }
        public ICustomerService CustomerService { get; }
        public IOrderService OrderService { get; }
        public IOrderDetailService OrderDetailService { get; }
        public IInvoiceService InvoiceService { get; }
        public IContractService ContractService { get; }
        public IQuotationService QuotationService { get; }
        public IQuotationDetailService QuotationDetailService { get; }
        public IDealerService DealerService { get; }
        public IPromotionService PromotionService { get; }
        public IUserProfileService UserProfileService { get; }
        public IVehicleService VehicleService { get; }
        public IVehicleModelService VehicleModelService { get; }
        public IVehicleVariantService VehicleVariantService { get; }
        public IWarehouseService WarehouseService { get; }
        public IVehicleTimeSlotService VehicleTimeSlotService { get; }
        public IMasterTimeSlotService MasterTimeSlotService { get; }
        public IAvailableSlotService AvailableSlotService { get; }
    public ITestDriveBookingService TestDriveBookingService { get; }
        public IEmailService EmailService { get; }
        public IStorageService StorageService { get; }
        public IDigitalSignatureService DigitalSignatureService { get; }

        public ServiceFacade(
            IAuthService authService,
            ICustomerService customerService,
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IInvoiceService invoiceService,
            IContractService contractService,
            IQuotationService quotationService,
            IQuotationDetailService quotationDetailService,
            IDealerService dealerService,
            IPromotionService promotionService,
            IUserProfileService userProfileService,
            IVehicleService vehicleService,
            IVehicleModelService vehicleModelService,
            IVehicleVariantService vehicleVariantService,
            IWarehouseService warehouseService,
            IVehicleTimeSlotService vehicleTimeSlotService,
            IMasterTimeSlotService masterTimeSlotService,
            IAvailableSlotService availableSlotService,
            ITestDriveBookingService testDriveBookingService,
            IEmailService emailService,
            IStorageService storageService,
            IDigitalSignatureService digitalSignatureService)
        {
            AuthService = authService;
            CustomerService = customerService;
            OrderService = orderService;
            OrderDetailService = orderDetailService;
            InvoiceService = invoiceService;
            ContractService = contractService;
            QuotationService = quotationService;
            QuotationDetailService = quotationDetailService;
            DealerService = dealerService;
            PromotionService = promotionService;
            UserProfileService = userProfileService;
            VehicleService = vehicleService;
            VehicleModelService = vehicleModelService;
            VehicleVariantService = vehicleVariantService;
            WarehouseService = warehouseService;
            VehicleTimeSlotService = vehicleTimeSlotService;
            MasterTimeSlotService = masterTimeSlotService;
            AvailableSlotService = availableSlotService;
            TestDriveBookingService = testDriveBookingService;
            EmailService = emailService;
            StorageService = storageService;
            DigitalSignatureService = digitalSignatureService;
        }
    }
}
