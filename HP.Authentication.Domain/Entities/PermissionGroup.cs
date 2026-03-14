using HP.Authentication.Domain.Utils;

namespace HP.Authentication.Domain.Entities
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
        public ICollection<UserPermissionGroup> UserPermissionGroups { get; set; } = new List<UserPermissionGroup>();
    }
}
