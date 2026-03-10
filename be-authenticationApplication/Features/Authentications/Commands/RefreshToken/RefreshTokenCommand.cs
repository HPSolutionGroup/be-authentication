using be_authenticationApplication.Features.Authentications.DTOs;
using MediatR;

namespace be_authenticationApplication.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<LoginResponse>
    {
        public string RefreshToken { get; set; } = null!;
        public Guid BranchId { get; set; }
    }
}
