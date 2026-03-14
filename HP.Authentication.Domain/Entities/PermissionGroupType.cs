using HP.Authentication.Domain.Utils;

namespace HP.Authentication.Domain.Entities
{
    public class PermissionGroupType : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }         // VD: "Hành chính", "Nhân sự", "Kế toán"
        public string? Description { get; set; }
        public ICollection<PermissionGroup> PermissionGroups { get; set; } = new List<PermissionGroup>();
    }
}
