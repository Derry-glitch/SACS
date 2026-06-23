using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SACS.Application.Common.Interfaces;
using SACS.Persistence.Contexts;

namespace SACS.Persistence.Contexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a default connection string if configuration is not loaded
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=SacsDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        builder.UseSqlServer(connectionString);

        return new ApplicationDbContext(builder.Options, new DesignTimeCurrentUserService());
    }

    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public string? UserId => "MigrationTime";
        public string? Email => "migration@sacs.com";
    }
}
