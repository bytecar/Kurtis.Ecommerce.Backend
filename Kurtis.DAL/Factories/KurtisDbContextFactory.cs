using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kurtis.DAL
{
    /// <summary>
    /// Provides design-time creation for EF Core migrations and CLI operations.
    /// </summary>
    public class KurtisDbContextFactory : IDesignTimeDbContextFactory<KurtisDbContext>
    {
        public KurtisDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KurtisDbContext>();
            var connectionString = configuration.GetConnectionString("KurtisDb")
                ?? "Server=SUNNYGAME,1433;Database=KurtisDB;User Id=sa;Password=Welcome$123;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new KurtisDbContext(optionsBuilder.Options);
        }
    }
}