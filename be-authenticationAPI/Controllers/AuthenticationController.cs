using be_authenticationApplication.Features.Authentications.Commands.Login;
using be_authenticationApplication.Features.Authentications.Commands.RefreshToken;
using be_localization.Abstractions;
using be_localization.Enums;
using be_localization.Web;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace be_authenticationAPI.Controllers
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
