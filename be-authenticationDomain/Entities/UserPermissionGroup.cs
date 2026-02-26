namespace be_authenticationDomain.Entities
{
    public class UserPermissionGroup
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; }

        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }
    }
}
