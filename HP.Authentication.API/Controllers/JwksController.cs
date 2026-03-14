using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using HP.Authentication.Localization.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HP.Authentication.API.Controllers
{
    [Route(".well-known")]
    [ApiController]
    public class JwksController : BaseLocalizedController
    {
        private readonly IJwtService _jwtService;

        public JwksController(
            IJsonLocalizationService localizer,
            IJwtService jwtService
            ) : base(localizer)

        {
            _jwtService = jwtService;
        }

        [HttpGet("jwks.json")]
        [AllowAnonymous]
        public IActionResult GetJwks()
        {
            var jwks = _jwtService.GetJwks();
            return CustomLocalizedResult("common", CommonKeys.DATA_LOADED_SUCCESSFULLY, jwks);
        }
    }
}
