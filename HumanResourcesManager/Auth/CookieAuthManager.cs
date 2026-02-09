using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace HumanResourcesManager.Auth
{
    public class CookieAuthManager
    {
        private readonly IHttpContextAccessor _http;

        public CookieAuthManager(IHttpContextAccessor http)
        {
            _http = http;
        }

        public async Task SignInAsync(List<Claim> claims)
        {
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await _http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }

        public async Task SignOutAsync()
        {
            await _http.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }
    }
}
