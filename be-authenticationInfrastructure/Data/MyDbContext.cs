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
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<CommandInFunction> CommandInFunctions { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupType> PermissionGroupTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }
        public DbSet<UserInBranch> UserInBranches { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserPermissionGroup> UserPermissionGroups { get; set; }

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

            #region Cấu hình UserInBranch
            builder.Entity<UserInBranch>()
                .HasKey(x => new { x.UserId, x.BranchId });

            builder.Entity<UserInBranch>()
                .HasOne(x => x.User)
                .WithMany(u => u.UserInBranches)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserInBranch>()
                .HasOne(x => x.Branch)
                .WithMany(b => b.UserInBranches)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Cấu hình UserPermission
            builder.Entity<UserPermission>()
                .HasKey(x => new { x.UserId, x.FunctionId, x.CommandId, x.BranchId });

            builder.Entity<UserPermission>()
                .HasOne(x => x.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermission>()
                .HasOne(x => x.Function)
                .WithMany()
                .HasForeignKey(x => x.FunctionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserPermission>()
                .HasOne(x => x.Command)
                .WithMany()
                .HasForeignKey(x => x.CommandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserPermission>()
                .HasOne(x => x.Branch)
                .WithMany(b => b.UserPermissions)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index tối ưu permission check
            builder.Entity<UserPermission>()
                .HasIndex(x => new { x.UserId, x.BranchId });

            #endregion

            #region UserPermissionGroup
            builder.Entity<UserPermissionGroup>()
                 .HasKey(x => new { x.UserId, x.PermissionGroupId, x.BranchId });

            builder.Entity<UserPermissionGroup>()
                .HasOne(x => x.User)
                .WithMany(u => u.UserPermissionGroups)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermissionGroup>()
                .HasOne(x => x.PermissionGroup)
                .WithMany(pg => pg.UserPermissionGroups)
                .HasForeignKey(x => x.PermissionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermissionGroup>()
                .HasOne(x => x.Branch)
                .WithMany(b => b.UserPermissionGroups)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermissionGroup>()
                .HasIndex(x => new { x.UserId, x.BranchId });
            #endregion

            #region CommandInFunction
            builder.Entity<CommandInFunction>()
                .HasKey(x => new { x.FunctionId, x.CommandId });

            builder.Entity<CommandInFunction>()
                .HasOne(x => x.Function)
                .WithMany(f => f.CommandInFunctions)
                .HasForeignKey(x => x.FunctionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommandInFunction>()
                .HasOne(x => x.Command)
                .WithMany(c => c.CommandInFunctions)
                .HasForeignKey(x => x.CommandId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Permission
            builder.Entity<Permission>()
                .HasOne(p => p.PermissionGroup)
                .WithMany(pg => pg.Permissions)
                .HasForeignKey(p => p.PermissionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Permission>()
                .HasOne(p => p.Function)
                .WithMany(f => f.Permissions)
                .HasForeignKey(p => p.FunctionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Permission>()
                .HasOne(p => p.Command)
                .WithMany(c => c.Permissions)
                .HasForeignKey(p => p.CommandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint - tránh trùng permission
            builder.Entity<Permission>()
                .HasIndex(p => new { p.PermissionGroupId, p.FunctionId, p.CommandId })
                .IsUnique();

            #endregion

            #region Subsystem
            builder.Entity<Function>()
                .HasOne(f => f.Subsystem)
                .WithMany(s => s.Functions)
                .HasForeignKey(f => f.SubsystemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Function>()
                .HasIndex(f => f.SubsystemId);
            #endregion

            #region Branch
            builder.Entity<Branch>()
                .HasKey(b => b.Id);
            #endregion

            #region PermissionGroup
            builder.Entity<PermissionGroup>()
                .HasOne(pg => pg.GroupType)
                .WithMany(gt => gt.PermissionGroups)
                .HasForeignKey(pg => pg.GroupTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PermissionGroup>()
                .HasIndex(pg => pg.GroupTypeId);
            #endregion

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
