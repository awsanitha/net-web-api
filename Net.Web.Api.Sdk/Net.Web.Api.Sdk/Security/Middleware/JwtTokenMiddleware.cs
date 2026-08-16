using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Middleware
{
    /// <summary>
    /// Middleware that validates JWT tokens from the Authorization header and sets the request principal.
    /// Replaces the legacy <c>DelegatingHandler</c>-based <c>JwtTokenHandler</c>.
    /// </summary>
    public class JwtTokenMiddleware
    {
        #region Private Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenMiddleware"/> class.
        /// </summary>
        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Middleware Entry Point

        /// <summary>
        /// Processes the HTTP request.
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

                    if (service != null)
                    {
                        var principal = tokenHandler.ValidateToken(
                            token, service.GetTokenValidationParameters(), out _);

                        context.User = principal;
                        Thread.CurrentPrincipal = principal;
                    }
                }
            }
            catch
            {
                // Token validation failure is non-fatal — the request continues unauthenticated.
            }

            await _next(context);
        }

        #endregion
    }
}
