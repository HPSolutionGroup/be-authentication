using be_authenticationApplication.Abstractions.Identity;
using be_authenticationApplication.Abstractions.Repository;
using be_authenticationApplication.Features.Authentications.DTOs;
using be_authenticationDomain.CustomException;
using be_authenticationDomain.Entities;
using be_localization.Abstractions;
using be_localization.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace be_authenticationApplication.Features.Authentications.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IJsonLocalizationService _localizer;
        private readonly IUnitOfWork _unitOfWork;
        public LoginCommandHandler(
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            UserManager<User> userManager,
            IJsonLocalizationService localizer,
            IUnitOfWork unitOfWork
            )
        {
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _userManager = userManager;
            _localizer = localizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository<User>()
                .Query()
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            if (user == null)
                throw new CustomException.UnAuthorizedException(_localizer.Get("auth", AuthKeys.INVALID_CREDENTIALS));

            if (!user.IsActive)
                throw new CustomException.UnAuthorizedException(_localizer.Get("auth", AuthKeys.USER_NOT_ACTIVE));

            var outcome = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash!, user);

            if (!outcome.IsSuccess)
            {
                await _userManager.AccessFailedAsync(user);
                throw new CustomException.UnAuthorizedException(_localizer.Get("auth", AuthKeys.INVALID_CREDENTIALS));
            }

            if (outcome.RequiresRehash)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                await _userManager.UpdateAsync(user);
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var jwtToken = _jwtService.GenerateJWTToken(user, roles, request.BranchId);

            var refreshToken = _jwtService.GenerateRefreshToken();

            await _userManager.SetAuthenticationTokenAsync(
                user,
                "HPAuthentication",
                "RefreshToken",
                refreshToken);

            return new LoginResponse
            {
                token = jwtToken,
                UserId = user.Id,
                UserName = user.Name ?? "Unknow",
                refreshToken = refreshToken,
                branchName = "",
                Avatar = user.Avatar,
            };
        }
    }
}
