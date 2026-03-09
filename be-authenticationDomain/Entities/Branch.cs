using be_authenticationDomain.Utils;

namespace be_authenticationDomain.Entities
{
    public class Branch : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }

        // ==== Navigate ====

        public ICollection<UserPermissionGroup> UserPermissionGroups { get; set; } = new List<UserPermissionGroup>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public ICollection<UserInBranch> UserInBranches { get; set; } = new List<UserInBranch>();
    }
}
