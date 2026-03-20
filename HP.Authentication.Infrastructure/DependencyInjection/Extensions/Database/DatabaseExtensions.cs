using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Abstractions.Repository.Authentication;
using HP.Authentication.Application.Abstractions.Repository.Authorization;
using HP.Authentication.Application.Abstractions.Repository.GenericRepository;
using HP.Authentication.Infrastructure.Data;
using HP.Authentication.Infrastructure.Integrations.Identity;
using HP.Authentication.Infrastructure.Integrations.Repository.Authentication;
using HP.Authentication.Infrastructure.Integrations.Repository.Authorization;
using HP.Authentication.Infrastructure.Integrations.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HP.Authentication.Infrastructure.DependencyInjection.Extensions.Database
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

            #region Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            #region Authentication
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            #endregion

            #region Authorization
            services.AddScoped<IUserSessionManager, UserSessionManager>();
            services.AddScoped<IUserInBranchRepository, UserInBranchRepository>();
            #endregion

            return services;
        }
    }
}
