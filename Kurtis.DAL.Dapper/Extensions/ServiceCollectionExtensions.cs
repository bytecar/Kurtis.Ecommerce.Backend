
using Microsoft.Extensions.DependencyInjection;
using Kurtis.DAL.Dapper.Infrastructure;
using Kurtis.DAL.Dapper.Repositories;
using Kurtis.DAL.Dapper.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace Kurtis.DAL.Dapper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKurtisDapper(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var cs = configuration.GetConnectionString("KurtisDb");
            if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("Connection string 'KurtisDb' not configured.");
            services.AddSingleton<IDbConnectionFactory>(sp => new SqlConnectionFactory(cs));
            services.AddScoped<ICatalogDapperRepository, CatalogDapperRepository>();
            return services;
        }

        public static IServiceCollection AddKurtisDapper(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            services.AddSingleton<IDbConnectionFactory>(sp => new SqlConnectionFactory(connectionString));
            services.AddScoped<ICatalogDapperRepository, CatalogDapperRepository>();
            return services;
        }
    }
}
