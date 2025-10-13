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

            services.AddBLLServices();

            // Register Repositories
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            // Register BLL Services
            services.AddBLLServices();

            // Register domain services
            services.AddScoped<IUserProfileService, UserProfileService>();

            // Add JWT Authentication
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
