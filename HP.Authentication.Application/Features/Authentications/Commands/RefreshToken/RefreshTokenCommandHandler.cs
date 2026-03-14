using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Common;
using HP.Authentication.Application.Features.Authentications.DTOs;
using HP.Authentication.Domain.CustomException;
using HP.Authentication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HP.Authentication.Application.Features.Authentications.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly IRefreshTokenManager _refreshTokenManager;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IUserContext _userContext;

        public RefreshTokenCommandHandler(
            IRefreshTokenManager refreshTokenManager,
            IJwtService jwtService,
            UserManager<User> userManager,
            IUserContext userContext)
        {
            _refreshTokenManager = refreshTokenManager;
            _jwtService = jwtService;
            _userManager = userManager;
            _userContext = userContext;
        }

        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new CustomException.InvalidDataException("Refresh token không được để trống.");

            var ipAddress = _userContext.GetIpAddress();

            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var user = await _refreshTokenManager.VerifyAndRotateTokenAsync(
                oldTokenString: request.RefreshToken,
                newTokenString: newRefreshToken,
                ipAddress: ipAddress);

            if (!user.IsActive)
                throw new CustomException.UnAuthorizedException("Tài khoản không hoạt động.");

            var roles = await _userManager.GetRolesAsync(user);

            var newJwtToken = _jwtService.GenerateJWTToken(user, roles, request.BranchId);

            return new LoginResponse
            {
                token = newJwtToken,
                refreshToken = newRefreshToken,
                UserId = user.Id,
                UserName = user.Name ?? "Unknown",
                Avatar = user.Avatar,
                branchName = ""
            };
        }
    }
}
