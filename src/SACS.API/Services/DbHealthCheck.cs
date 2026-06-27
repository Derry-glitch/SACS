using Microsoft.Extensions.Diagnostics.HealthChecks;
using SACS.Persistence.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SACS.API.Services;

public class DbHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DbHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect 
                ? HealthCheckResult.Healthy("Database is responding.")
                : HealthCheckResult.Unhealthy("Database connection failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database connection error: {ex.Message}");
        }
    }
}
