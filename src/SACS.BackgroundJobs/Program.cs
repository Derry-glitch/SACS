using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using SACS.Persistence;
using SACS.Infrastructure;
using SACS.Application.Common.Interfaces;
using SACS.BackgroundJobs.Services;

namespace SACS.BackgroundJobs;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                // Register database, unit of work, and repository services
                services.AddPersistenceServices(configuration);

                // Register caching, files, event bus, JWT, and notifications
                services.AddInfrastructureServices(configuration);

                // Configure current user service for background context
                services.AddScoped<ICurrentUserService, BackgroundCurrentUserService>();

                // Configure Hangfire using SQL Server storage
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")
                        ?? "Server=(localdb)\\mssqllocaldb;Database=SacsDb;Trusted_Connection=True;MultipleActiveResultSets=true"));

                // Run Hangfire Server within this worker host
                services.AddHangfireServer();
            })
            .Build();

        host.Run();
    }
}
