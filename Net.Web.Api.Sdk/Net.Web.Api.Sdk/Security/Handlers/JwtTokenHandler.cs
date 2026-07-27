using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenMiddleware.
    /// Middleware to validate JWT tokens and set the current user principal.
    /// </summary>
    public class JwtTokenHandler : IMiddleware
    {
        #region IMiddleware Implementations

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var token = context.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var service = context.RequestServices.GetService<IJwtTokenService>();

                    if (service != null)
                    {
                        var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);

                        if (principal != null)
                        {
                            context.User = principal;
                        }
                    }
                }
            }
            catch
            {
                // ignored - unauthenticated requests continue normally
            }

            await next(context);
        }

        #endregion
    }
}
