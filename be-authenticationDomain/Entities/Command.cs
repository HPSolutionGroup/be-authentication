using be_authenticationDomain.Utils;

namespace be_authenticationDomain.Entities
{
    public class Command : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<CommandInFunction> CommandInFunctions { get; set; }
    }
}
