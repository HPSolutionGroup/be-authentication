using be_authenticationApplication.Abstractions.Identity;
using be_authenticationDomain.Entities;
using be_authenticationInfrastructure.Data;
using be_authenticationInfrastructure.Integrations.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace be_authenticationInfrastructure.DependencyInjection.Extensions.Authentication
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityExtension(this IServiceCollection services, IConfiguration configuration)
        {      
            services.AddSingleton<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher,PasswordHasher>();
            services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
            services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();

            var publicKeyPath = configuration["Jwt:PublicKeyPath"];
            var rsa = RSA.Create();

            if (!string.IsNullOrWhiteSpace(publicKeyPath) && File.Exists(publicKeyPath))
            {
                rsa.ImportFromPem(File.ReadAllText(publicKeyPath).ToCharArray());
            }

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<MyDbContext>()
                .AddDefaultTokenProviders();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new RsaSecurityKey(rsa)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/notification"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
