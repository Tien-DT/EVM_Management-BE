using Microsoft.Extensions.DependencyInjection;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.Services.Class;

namespace EVMManagement.BLL.Services
{
    public static class ServiceProvider
    {
        public static IServiceCollection AddBLLServices(this IServiceCollection services)
        {
            // Register Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStorageService, SupabaseStorageService>();
            return services;
        }
    }
}