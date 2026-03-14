using Microsoft.AspNetCore.Identity;

namespace HP.Authentication.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // ==== Navigate ====

        public ICollection<UserPermissionGroup> UserPermissionGroups { get; set; } = new List<UserPermissionGroup>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public ICollection<UserInBranch> UserInBranches { get; set; } = new List<UserInBranch>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    }
}
