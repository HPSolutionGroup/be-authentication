using be_authenticationDomain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace be_authenticationInfrastructure.Data
{
    public class MyDbContext : IdentityDbContext<User, Role, Guid>
    {
        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        #region DbSet

        #endregion
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
            }
        }

        #region Migration - Seed
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Configure Relationships
            #endregion

            #region Migration DaTa Defaults

            #region Dummy Data Default Role-User

            var roleSuperAdminID = Guid.NewGuid();
            var roleAdminID = Guid.NewGuid();
            var roleUserID = Guid.NewGuid();
            var supperAdminID = Guid.NewGuid();
            var adminID = Guid.NewGuid();

            builder.Entity<Role>().HasData(
                new Role
                {
                    Id = roleSuperAdminID,
                    Name = "SuperAdmin",
                    NormalizedName = "SUPER_ADMIN"
                },
                new Role
                {
                    Id = roleAdminID,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new Role
                {
                    Id = roleUserID,
                    Name = "User",
                    NormalizedName = "USER",

                }
            );

            var hasher = new PasswordHasher<User>();
            builder.Entity<User>().HasData(
                new User
                {
                    Id = supperAdminID,
                    Name = "Super_Admin",
                    UserName = "superadmin@gmail.com",
                    Avatar = "https://res.cloudinary.com/da3m7fj99/image/upload/v1732819079/admin_hpdxlr.png",
                    NormalizedUserName = "SUPERADMIN@GMAIL.COM",
                    Email = "superadmin@gmail.com",
                    PasswordHash = hasher.HashPassword(null, "SuperAdmin@789"),
                    EmailConfirmed = true,
                    SecurityStamp = "N9WM7PKRYL4J3AV26GTXUEQB0CZMFH51",
                    IsActive = true
                },
                new User
                {
                    Id = adminID,
                    Name = "Admin",
                    UserName = "admin@gmail.com",
                    Avatar = "https://res.cloudinary.com/da3m7fj99/image/upload/v1732819079/admin_hpdxlr.png",
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    Email = "admin@gmail.com",
                    PasswordHash = hasher.HashPassword(null, "Admin@123"),
                    EmailConfirmed = true,
                    SecurityStamp = "VHHP3SM5ARZNAMM6YNEZY6SQXWQ6YYIJ",
                    IsActive = true
                }
            );

            // Cấu hình IdentityUserRole
            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new
                {
                    UserId = supperAdminID,
                    RoleId = roleSuperAdminID,
                },
                new IdentityUserRole<Guid>
                {
                    UserId = adminID,
                    RoleId = roleAdminID
                }
            );
            #endregion

            #endregion
        }
        #endregion
    }
}
