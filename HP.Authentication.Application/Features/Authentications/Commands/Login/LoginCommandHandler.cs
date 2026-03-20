using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Abstractions.Repository.Authorization;
using HP.Authentication.Application.Abstractions.Repository.GenericRepository;
using HP.Authentication.Application.Common;
using HP.Authentication.Application.Features.Authentications.DTOs;
using HP.Authentication.Domain.CustomException;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HP.Authentication.Application.Features.Authentications.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IJsonLocalizationService _localizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenManager _refreshTokenManager;
        private readonly IUserContext _userContext;
        private readonly IUserSessionManager _userSessionManager;
        private readonly IUserInBranchRepository _userInBranchRepository;
        public LoginCommandHandler(
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            UserManager<User> userManager,
            IJsonLocalizationService localizer,
            IUnitOfWork unitOfWork,
            IRefreshTokenManager refreshTokenManager,
            IUserContext userContext,
            IUserSessionManager userSessionManager,
            IUserInBranchRepository userInBranchRepository)
        {
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _userManager = userManager;
            _localizer = localizer;
            _unitOfWork = unitOfWork;
            _refreshTokenManager = refreshTokenManager;
            _userContext = userContext;
            _userSessionManager = userSessionManager;
            _userInBranchRepository = userInBranchRepository;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository<User>()
                .Query(false)
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

            var selectedBranch = await _unitOfWork.Repository<Branch>()
                .Query(false)
                .Where(b => b.Id == request.BranchId)
                .Select(b => new { b.Name })
                .FirstOrDefaultAsync();

            var allowedBranches = await _userInBranchRepository.GetBranchesOfUserAsync(user.Id);
            var allowedBranchIds = allowedBranches.Select(b => b.BranchId).ToList();

            if (!allowedBranchIds.Contains(request.BranchId))
                throw new CustomException.UnAuthorizedException(_localizer.Get("auth", AuthKeys.BRANCH_ACCESS_DENIED));

            if (selectedBranch == null)
                throw new CustomException.UnAuthorizedException(_localizer.Get("auth", AuthKeys.BRANCH_NOT_FOUND));

            var roles = await _userManager.GetRolesAsync(user);

            var jwtToken = _jwtService.GenerateJWTToken(user, roles, request.BranchId);

            var refreshToken = _jwtService.GenerateRefreshToken();

            #region Relotation with Identity - NonUses
            //await _userManager.SetAuthenticationTokenAsync(
            //    user,
            //    "HPAuthentication",
            //    "RefreshToken",
            //    refreshToken);
            #endregion

            var ipAddress = _userContext.GetIpAddress();
            var userAgent = _userContext.GetUserAgent();    
            var deviceName = _userContext.GetDeviceName();
            var deviceId = _userContext.GetDeviceId();

            var session = await _userSessionManager.CreateSessionAsync(
                user.Id,
                ipAddress,
                userAgent,
                deviceName,
                deviceId
            );

            await _refreshTokenManager.CreateTokenAsync(
                userId: user.Id,
                tokenString: refreshToken,
                ipAddress: ipAddress ?? "unknown",
                sessionId: session.Id
            );

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
