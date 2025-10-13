using EVMManagement.API.Setup;

namespace EVMManagement.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container and configure JSON to serialize enums as strings
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            
            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Database Configuration
            builder.Services.AddDatabaseConfiguration(builder.Configuration);

            // Add Cache Configuration (Redis)
            builder.Services.AddCacheConfiguration(builder.Configuration);

            // Add Dependency Injection (UnitOfWork, Services, JWT)
            builder.Services.AddDependencyInjection(builder.Configuration);

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Apply migrations
            await app.ApplyMigrationsAsync();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable Swagger in Production for Render.com
            if (app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Ok(new
            {
                service = "EVM Management API",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName,
                version = "1.0.0"
            })).AllowAnonymous();

            app.MapGet("/health", () => Results.Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName
            })).AllowAnonymous();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
