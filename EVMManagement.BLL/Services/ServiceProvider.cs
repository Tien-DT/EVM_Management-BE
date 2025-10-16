using Microsoft.Extensions.DependencyInjection;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.Services.Class;

namespace EVMManagement.BLL.Services
{
    public static class ServiceProvider
    {
        public static IServiceCollection AddBLLServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStorageService, SupabaseStorageService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IQuotationDetailService, QuotationDetailService>();
            services.AddScoped<IContractService, ContractService>();
            return services;
        }
    }
}
