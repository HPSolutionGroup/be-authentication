using be_authenticationApplication.Abstractions.Identity;
using be_localization.Abstractions;
using be_localization.Enums;
using be_localization.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_authenticationAPI.Controllers
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
