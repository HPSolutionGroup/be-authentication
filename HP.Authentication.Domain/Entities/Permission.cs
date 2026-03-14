namespace HP.Authentication.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; }

        public Guid FunctionId { get; set; }
        public Function Function { get; set; }

        public Guid CommandId { get; set; }
        public Command Command { get; set; }
    }
}
