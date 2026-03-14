using HP.Authentication.Application.Features.Authentications.DTOs;
using MediatR;

namespace HP.Authentication.Application.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<LoginResponse>
    {
        public string RefreshToken { get; set; } = null!;
        public Guid BranchId { get; set; }
    }
}
