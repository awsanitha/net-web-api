using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenMiddleware. Validates JWT bearer tokens in incoming requests.
    /// </summary>
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate.</param>
        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="tokenService">The JWT token service.</param>
        public async Task InvokeAsync(HttpContext context, IJwtTokenService tokenService)
        {
            try
            {
                var authHeader = context.Request.Headers.Authorization.ToString();

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var rawToken = authHeader.Substring("Bearer ".Length).Trim();
                    var token = rawToken.GetToken(out var securityToken);

                    if (!string.IsNullOrEmpty(token) && securityToken != null)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var validationParameters = tokenService.GetTokenValidationParameters();
                        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                        context.User = principal;
                    }
                }
            }
            catch
            {
                // Ignored — invalid tokens result in unauthenticated request, handled by auth filters
            }

            await _next(context);
        }
    }
}
