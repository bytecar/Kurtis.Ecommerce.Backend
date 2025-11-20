
using Microsoft.Extensions.DependencyInjection;
using Kurtis.DAL.Repositories;
using Kurtis.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kurtis.DAL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKurtisDal(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<KurtisDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("KurtisDb")));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IRecentlyViewedRepository, RecentlyViewedRepository>();
            services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
            services.AddScoped<IReturnRepository, ReturnRepository>();
            return services;
        }

        public static IServiceCollection AddKurtisDal(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<KurtisDbContext>(opt => opt.UseSqlServer(connectionString));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IRecentlyViewedRepository, RecentlyViewedRepository>();
            services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
            services.AddScoped<IReturnRepository, ReturnRepository>();
            return services;
        }
    }
}
