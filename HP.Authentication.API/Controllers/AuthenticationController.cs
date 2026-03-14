using HP.Authentication.Application.Features.Authentications.Commands.Login;
using HP.Authentication.Application.Features.Authentications.Commands.RefreshToken;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using HP.Authentication.Localization.Web;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HP.Authentication.API.Controllers
{
    [Route("api/v1/authentication")]
    [ApiController]
    public class AuthenticationController : BaseLocalizedController
    {
        private readonly IMediator _mediator;

        public AuthenticationController(
            IJsonLocalizationService localizer,
            IMediator mediator
            ) : base(localizer)
        {
            _mediator = mediator;
        }

        #region Đăng nhập
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Đăng nhập",
            Description = "Đăng nhập bằng tài khoản và mật khẩu ."
        )]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CustomLocalizedResult("auth", AuthKeys.LOGIN_SUCCESS, result);
        }
        #endregion

        #region Refresh Token

        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "Làm mới access token",
            Description = "Sử dụng refresh token để cấp lại access token và refresh token mới."
        )]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CustomLocalizedResult("auth", AuthKeys.REFRESH_SUCCESS, result);
        }

        #endregion
    }
}
