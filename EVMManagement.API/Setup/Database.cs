using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;

namespace EVMManagement.API.Setup
{
    public static class Database
    {
        public static IServiceCollection AddDatabaseConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configure PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection_Local"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                        npgsqlOptions.MigrationsAssembly("EVMManagement.DAL");
                    });
            });

            return services;
        }

        public static async Task<IApplicationBuilder> ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            try
            {
                // Apply pending migrations
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }

            return app;
        }
    }
}
