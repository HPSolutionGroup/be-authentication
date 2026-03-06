using be_authenticationApplication.Abstractions.Repository;
using be_authenticationInfrastructure.Data;
using be_authenticationInfrastructure.Integrations.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be_authenticationInfrastructure.DependencyInjection.Extensions.Database
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseExtension(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext
            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Default"),
                    b => b.MigrationsAssembly(typeof(DatabaseExtensions).Assembly.FullName)
                ), ServiceLifetime.Scoped);

            // Đăng ký Repository và UnitOfWork
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
