using be_authenticationDomain.Entities;
using HP.Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HP.Authentication.Infrastructure.Data
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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
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
        public DbSet<UserSession> UserSessions { get; set; }

        #endregion

        #region Migration - Seed
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Configure Relationships

            #region Branch
            builder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(255);
                entity.Property(b => b.Address).HasMaxLength(500);
            });
            #endregion

            #region Command
            builder.Entity<Command>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(30);
            });
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

            #region Function
            builder.Entity<Function>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(255);
                entity.Property(x => x.Code).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Url).HasMaxLength(255);

                entity.HasIndex(x => x.Code).IsUnique();
                entity.HasIndex(f => f.SubsystemId);

                entity.HasOne<Function>()
                    .WithMany()
                    .HasForeignKey(x => x.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Subsystem)
                      .WithMany(s => s.Functions)
                      .HasForeignKey(f => f.SubsystemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Permission
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(p => p.PermissionGroup)
                    .WithMany(pg => pg.Permissions)
                    .HasForeignKey(p => p.PermissionGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Function)
                    .WithMany(f => f.Permissions)
                    .HasForeignKey(p => p.FunctionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Command)
                    .WithMany(c => c.Permissions)
                    .HasForeignKey(p => p.CommandId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => new { p.PermissionGroupId, p.FunctionId, p.CommandId }).IsUnique();
            });
            #endregion

            #region PermissionGroup

            builder.Entity<PermissionGroup>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(255);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.HasIndex(pg => pg.GroupTypeId);
            });

            builder.Entity<PermissionGroup>()
                .HasOne(pg => pg.GroupType)
                .WithMany(gt => gt.PermissionGroups)
                .HasForeignKey(pg => pg.GroupTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PermissionGroup>()
                .HasIndex(pg => pg.GroupTypeId);
            #endregion

            #region PermissionGroupType
            builder.Entity<PermissionGroupType>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(255);
                entity.Property(x => x.Description).HasMaxLength(500);
            });
            #endregion

            #region RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.CreatedByIp)
                    .HasMaxLength(50);

                entity.Property(x => x.RevokedByIp)
                    .HasMaxLength(50);

                entity.Property(x => x.ReasonRevoked)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Token)
                    .IsUnique();

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.SessionId);
                entity.HasIndex(x => x.FamilyId);
                entity.HasIndex(x => x.ExpiresAt);
                entity.HasIndex(x => x.ReplacedByTokenId);
                entity.HasIndex(x => x.ParentTokenId);
                entity.HasIndex(x => x.Token).IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Session)
                    .WithMany(s => s.RefreshTokens)
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Cấu hình Subsystem 
            builder.Entity<Subsystem>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(255);
                entity.Property(x => x.Code).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.HasIndex(x => x.Code).IsUnique();
            });

            #endregion

            #region User
            builder.Entity<User>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(255);
                entity.Property(x => x.Avatar).HasMaxLength(500);
            });
            #endregion

            #region UserInBranch
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

            #region UserPermission
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

            #region UserSession
            builder.Entity<UserSession>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.DeviceName)
                    .HasMaxLength(255);

                entity.Property(x => x.DeviceId)
                    .HasMaxLength(255);

                entity.Property(x => x.UserAgent)
                    .HasMaxLength(1000);

                entity.Property(x => x.IpAddress)
                    .HasMaxLength(50);

                entity.Property(x => x.RevokedReason)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.RevokedAt);

                entity.HasOne(x => x.User)
                    .WithMany(u => u.UserSessions)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #endregion

            #region Migration DaTa Defaults

            #region Dummy Data Default Role-User

            var roleSuperAdminID = Guid.Parse("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D");
            var roleAdminID = Guid.Parse("B2C3D4E5-F6A7-4B8C-9D0E-1F2A3B4C5D6E");
            var roleUserID = Guid.Parse("C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F");

            var supperAdminID = Guid.Parse("D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F8A");
            var adminID = Guid.Parse("E5F6A7B8-C9D0-4E1F-2A3B-4C5D6E7F8A9B");

            builder.Entity<Role>().HasData(
                new Role
                {
                    Id = roleSuperAdminID,
                    Name = "SuperAdmin",
                    NormalizedName = "SUPER_ADMIN",
                    ConcurrencyStamp = "J7XQ2P7ZNCL5BKA3YTR2WJDF6G5VHS7E"
                },
                new Role
                {
                    Id = roleAdminID,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "M2NC4P2XQZ5LKA2YTR7WJDF3G6VHS5EM"
                },
                new Role
                {
                    Id = roleUserID,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "V5XQ2P7ZNCL5BKA3YTR2WJDF6G5VHS7A"
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
                    ConcurrencyStamp = "B7NC4P2XQZ5LKA2YTR7WJDF3G6VHS5EM",
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
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin@123"),
                    EmailConfirmed = true,
                    SecurityStamp = "VHHP3SM5ARZNAMM6YNEZY6SQXWQ6YYIJ",
                    ConcurrencyStamp = "P4XQ2P7ZNCL5BKA3YTR2WJDF6G5VHS7E",
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
