using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace EVMManagement.API.Setup
{
    public static class Cache
    {
        public static IServiceCollection AddCacheConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["RedisSettings:ConnectionString"];
            var instanceName = configuration["RedisSettings:InstanceName"];

            if (!string.IsNullOrEmpty(connectionString))
            {
                // Add Redis distributed cache
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connectionString;
                    options.InstanceName = instanceName;
                });

                // Add IConnectionMultiplexer for direct Redis operations
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var config = ConfigurationOptions.Parse(connectionString);
                    config.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(config);
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis is not configured
                services.AddDistributedMemoryCache();
            }

            return services;
        }
    }
}
