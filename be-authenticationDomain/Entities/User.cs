using Microsoft.AspNetCore.Identity;

namespace be_authenticationDomain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // ==== Navigate ====

        public ICollection<UserPermissionGroup> UserPermissionGroups { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
        public ICollection<UserInBranch> UserInBranches { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
