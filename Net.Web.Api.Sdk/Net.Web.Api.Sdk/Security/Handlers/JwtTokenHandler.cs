using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenMiddleware. Validates JWT tokens and sets the principal.
    /// </summary>
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenMiddleware"/> class.
        /// </summary>
        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                    var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);

                    context.User = principal;
                }
            }
            catch
            {
                // ignored - let the request continue; authorization filters will reject if needed
            }

            await _next(context);
        }
    }
}
