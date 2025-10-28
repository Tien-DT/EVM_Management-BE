using System.Linq;
using AutoMapper;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Request.Dealer;
using EVMManagement.BLL.DTOs.Request.Deposit;
using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Request.HandoverRecord;
using EVMManagement.BLL.DTOs.Request.Invoice;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Request.OrderDetail;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Request.Quotation;
using EVMManagement.BLL.DTOs.Request.QuotationDetail;
using EVMManagement.BLL.DTOs.Request.Report;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Request.User;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response.Deposit;
using EVMManagement.BLL.DTOs.Response.DigitalSignature;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;
using EVMManagement.BLL.DTOs.Response.Invoice;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.DTOs.Response.OrderDetail;
using EVMManagement.BLL.DTOs.Response.Promotion;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.BLL.DTOs.Response.QuotationDetail;
using EVMManagement.BLL.DTOs.Response.Report;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using UserDealerDto = EVMManagement.BLL.DTOs.Response.User.DealerDto;
using TransportResponse = EVMManagement.BLL.DTOs.Response.Transport.TransportResponseDto;
using TransportDetailResponse = EVMManagement.BLL.DTOs.Response.Transport.TransportDetailDto;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // MasterTimeSlot Mappings
            CreateMap<MasterTimeSlotCreateDto, MasterTimeSlot>();
            CreateMap<MasterTimeSlotUpdateDto, MasterTimeSlot>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MasterTimeSlot, MasterTimeSlotResponseDto>();

            // VehicleModel Mappings
            CreateMap<VehicleModelCreateDto, VehicleModel>();
            CreateMap<VehicleModelUpdateDto, VehicleModel>();
            CreateMap<VehicleModel, VehicleModelResponseDto>();

            // Vehicle Mappings
            CreateMap<VehicleCreateDto, Vehicle>();
            CreateMap<VehicleUpdateDto, Vehicle>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Vehicle, VehicleResponseDto>();
            CreateMap<Vehicle, VehicleDetailResponseDto>()
                .ForMember(dest => dest.Warehouse, opt => opt.MapFrom(src => src.Warehouse))
                .ForMember(dest => dest.Dealer, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Dealer : null));
            CreateMap<Warehouse, WarehouseDetailDto>();
            CreateMap<Dealer, DealerDetailDto>();

            // VehicleVariant Mappings
            CreateMap<VehicleVariantCreateDto, VehicleVariant>();
            CreateMap<VehicleVariantUpdateDto, VehicleVariant>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<VehicleVariant, VehicleVariantResponse>();
            CreateMap<VehicleVariant, VehicleVariantDetailDto>();
            CreateMap<VehicleModel, VehicleModelDetailDto>();

            // Warehouse Mappings
            CreateMap<WarehouseCreateDto, Warehouse>();
            CreateMap<WarehouseUpdateDto, Warehouse>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Warehouse, WarehouseResponseDto>();
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<VehicleVariant, VehicleVariantDto>();
            CreateMap<VehicleModel, VehicleModelDto>();

            // Customer Mappings
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Customer, CustomerResponse>();

            // Dealer Mappings
            CreateMap<CreateDealerDto, Dealer>();
            CreateMap<UpdateDealerDto, Dealer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Dealer, DealerResponseDto>();
            CreateMap<Dealer, UserDealerDto>();

            // Promotion Mappings
            CreateMap<PromotionCreateDto, Promotion>();
            CreateMap<PromotionUpdateDto, Promotion>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Promotion, PromotionResponseDto>();

            // VehicleTimeSlot Mappings
            CreateMap<VehicleTimeSlotCreateDto, VehicleTimeSlot>();
            CreateMap<VehicleTimeSlotUpdateDto, VehicleTimeSlot>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<VehicleTimeSlot, VehicleTimeSlotResponseDto>();

            // AvailableSlot Mappings removed - now using VehicleTimeSlot with Status = AVAILABLE

            // TestDriveBooking Mappings
            CreateMap<TestDriveBookingCreateDto, TestDriveBooking>();
            CreateMap<TestDriveBookingUpdateDto, TestDriveBooking>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TestDriveBooking, TestDriveBookingResponseDto>();

            // Order Mappings
            CreateMap<OrderCreateDto, Order>();
            CreateMap<OrderWithDetailsCreateDto, Order>()
                .IncludeBase<OrderCreateDto, Order>()
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());
            CreateMap<OrderUpdateDto, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Order, OrderResponse>();
            CreateMap<Order, OrderWithDetailsResponse>()
                .IncludeBase<Order, OrderResponse>();

            // OrderDetail Mappings
            CreateMap<OrderDetailCreateDto, OrderDetail>();
            CreateMap<OrderDetailForOrderCreateDto, OrderDetail>();
            CreateMap<OrderDetailUpdateDto, OrderDetail>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderDetail, OrderDetailResponse>();

            // Quotation Mappings
            CreateMap<CreateQuotationDto, Quotation>();
            CreateMap<UpdateQuotationDto, Quotation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Quotation, QuotationResponseDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Phone : null))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null))
                .ForMember(dest => dest.QuotationDetails, opt => opt.MapFrom(src => src.QuotationDetails));

            // QuotationDetail Mappings
            CreateMap<QuotationDetailCreateDto, QuotationDetail>();
            CreateMap<QuotationDetailUpdateDto, QuotationDetail>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<QuotationDetail, QuotationDetailResponse>();
            CreateMap<QuotationDetail, QuotationDetailResponseDto>()
                .ForMember(dest => dest.VehicleVariantColor, opt => opt.MapFrom(src => src.VehicleVariant != null ? src.VehicleVariant.Color : null))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.VehicleVariant != null && src.VehicleVariant.VehicleModel != null ? src.VehicleVariant.VehicleModel.Name : null))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => System.Math.Round(src.UnitPrice * src.Quantity * (1m - src.DiscountPercent / 100m), 2)));

            // Contract Mappings
            CreateMap<ContractCreateDto, Contract>();
            CreateMap<ContractUpdateDto, Contract>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Contract, ContractResponse>();
            CreateMap<Contract, ContractDetailResponse>();

            // Invoice Mappings
            CreateMap<InvoiceCreateDto, Invoice>();
            CreateMap<InvoiceUpdateDto, Invoice>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Invoice, InvoiceResponse>();

            // DealerContract Mappings
            CreateMap<DealerContractCreateDto, DealerContract>();
            CreateMap<DealerContract, DealerContractResponseDto>();

            // UserProfile Mappings
            CreateMap<UserProfileUpdateDto, UserProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Account, AccountDto>();
            CreateMap<UserProfile, UserProfileResponse>();

            // HandoverRecord Mappings
            CreateMap<HandoverRecordCreateDto, HandoverRecord>();
            CreateMap<HandoverRecordUpdateDto, HandoverRecord>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<HandoverRecord, HandoverRecordResponseDto>();
            CreateMap<TransportDetail, TransportDetailResponse>()
                .ForMember(dest => dest.VehicleVin, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Vin : null))
                .ForMember(dest => dest.VehicleVariantName, opt => opt.MapFrom(src =>
                    src.Vehicle != null && src.Vehicle.VehicleVariant != null
                        ? string.Join(" - ", new[]
                        {
                            src.Vehicle.VehicleVariant.VehicleModel != null ? src.Vehicle.VehicleVariant.VehicleModel.Name : null,
                            src.Vehicle.VehicleVariant.Color
                        }.Where(v => !string.IsNullOrWhiteSpace(v)))
                        : null))
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.Order != null ? src.Order.Code : null));
            CreateMap<Transport, TransportResponse>()
                .ForMember(dest => dest.TransportDetails, opt => opt.MapFrom(src => src.TransportDetails))
                .ForMember(dest => dest.DealerId, opt => opt.MapFrom(src => src.TransportDetails.Where(td => td.Order != null && td.Order.DealerId.HasValue).Select(td => td.Order!.DealerId).FirstOrDefault()))
                .ForMember(dest => dest.DealerName, opt => opt.MapFrom(src => src.TransportDetails.Where(td => td.Order != null && td.Order.Dealer != null).Select(td => td.Order!.Dealer!.Name).FirstOrDefault()))
                .ForMember(dest => dest.DealerAddress, opt => opt.MapFrom(src => src.TransportDetails.Where(td => td.Order != null && td.Order.Dealer != null).Select(td => td.Order!.Dealer!.Address).FirstOrDefault()));

            // DigitalSignature Mappings
            CreateMap<DigitalSignature, DigitalSignatureResponse>();

            // Report Mappings
            CreateMap<ReportCreateDto, Report>();
            CreateMap<Report, ReportResponse>();

            // Deposit Mappings
            CreateMap<DepositCreateDto, Deposit>();
            CreateMap<Deposit, DepositResponse>();
        }
    }
}
