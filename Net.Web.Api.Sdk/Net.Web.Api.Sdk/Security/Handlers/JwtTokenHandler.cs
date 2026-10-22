using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// ASP.NET Core middleware that validates JWT bearer tokens from the Authorization header
    /// and sets the current user principal.
    /// Replaces the former DelegatingHandler-based JwtTokenHandler.
    /// </summary>
    public class JwtTokenMiddleware
    {
        #region Private Properties

        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Middleware Implementations

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
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
                        var service = context.RequestServices.GetService<IJwtTokenService>();

                        if (service != null)
                        {
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var principal = tokenHandler.ValidateToken(
                                token,
                                service.GetTokenValidationParameters(),
                                out _);

                            context.User = principal;
                        }
                    }
                }
            }
            catch
            {
                // Ignored – let the authorization filter handle failures
            }

            await _next(context);
        }

        #endregion
    }
}
