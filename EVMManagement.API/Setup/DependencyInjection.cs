using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Repositories.Class;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.Services.Class;

namespace EVMManagement.API.Setup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<GmailApiSettings>(configuration.GetSection(GmailApiSettings.SectionName));

            services.AddBLLServices();

            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IVehicleVariantRepository, VehicleVariantRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IDealerRepository, DealerRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IVehicleTimeSlotRepository, VehicleTimeSlotRepository>();
            services.AddScoped<IMasterTimeSlotRepository, MasterTimeSlotRepository>();
            services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();

            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IVehicleVariantService, VehicleVariantService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleModelService, VehicleModelService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IDealerService, DealerService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IVehicleTimeSlotService, VehicleTimeSlotService>();
            services.AddScoped<IMasterTimeSlotService, MasterTimeSlotService>();
            services.AddScoped<IAvailableSlotService, AvailableSlotService>();
            services.AddScoped<IDealerContractService, DealerContractService>();

            // add JWT auth
            AddJwtAuthentication(services, configuration);

            return services;
        }

        private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:SecretKey"];
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured properly.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}
