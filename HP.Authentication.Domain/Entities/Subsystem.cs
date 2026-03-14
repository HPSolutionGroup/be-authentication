using HP.Authentication.Domain.Utils;

namespace HP.Authentication.Domain.Entities
{
    public class Subsystem : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }

        // ==== Navigate ====

        public ICollection<Function> Functions { get; set; } = new List<Function>();
    } 
}
