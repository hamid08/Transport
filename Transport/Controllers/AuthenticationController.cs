using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OAuth.OpenIddict.AuthorizationServer.Controllers
{
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthenticationController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager)
        {
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

           


            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                              throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

          
            var userId = "2";

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, userId)
                .SetClaim(Claims.Email, userId)
                .SetClaim(Claims.Name, userId)
                .SetClaims(Claims.Role, new List<string> { "user", "admin" }.ToImmutableArray());

            identity.SetScopes(request.GetScopes());

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
                throw new InvalidOperationException("The specified grant type is not supported.");

            var result =
                await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var userId = result.Principal.GetClaim(Claims.Subject);


            var identity = new ClaimsIdentity(result.Principal.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, userId)
                .SetClaim(Claims.Email, userId)
                .SetClaim(Claims.Name, userId)
                .SetClaims(Claims.Role, new List<string> { "user", "admin" }.ToImmutableArray());

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }
    }
}
