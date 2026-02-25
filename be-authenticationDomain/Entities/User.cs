using Microsoft.AspNetCore.Identity;

namespace be_authenticationDomain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
