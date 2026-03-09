using be_authenticationDomain.Utils;

namespace be_authenticationDomain.Entities
{
    public class Command : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        // ==== Navigate ====
        public ICollection<CommandInFunction> CommandInFunctions { get; set; } = new List<CommandInFunction>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    }
}
