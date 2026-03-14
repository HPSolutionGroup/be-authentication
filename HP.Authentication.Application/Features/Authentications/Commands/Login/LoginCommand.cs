using HP.Authentication.Application.Features.Authentications.DTOs;
using MediatR;

namespace HP.Authentication.Application.Features.Authentications.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
    }
}
