using be_authenticationDomain.Utils;

namespace be_authenticationDomain.Entities
{
    public class PermissionGroup : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        // ==== Navigate ====

        public Guid GroupTypeId { get; set; }
        public PermissionGroupType GroupType { get; set; }

        public ICollection<Permission> Permissions { get; set; }
        public ICollection<UserPermissionGroup> UserPermissionGroups { get; set; }
    }
}
