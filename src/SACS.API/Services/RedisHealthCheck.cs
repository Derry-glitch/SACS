using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SACS.API.Services;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHostEnvironment _env;

    public RedisHealthCheck(IConnectionMultiplexer redis, IHostEnvironment env)
    {
        _redis = redis;
        _env = env;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            await database.PingAsync();
            return HealthCheckResult.Healthy("Redis is responding.");
        }
        catch (Exception ex)
        {
            if (_env.IsDevelopment())
            {
                return HealthCheckResult.Degraded($"Redis is offline in Development (tolerated): {ex.Message}");
            }
            return HealthCheckResult.Unhealthy($"Redis connection error: {ex.Message}");
        }
    }
}
