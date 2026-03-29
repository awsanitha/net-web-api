using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenHandler - ASP.NET Core middleware for JWT token processing.
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
                    var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                    var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);

                    context.User = principal;
                }
            }
            catch
            {
                // ignored - let the request continue unauthenticated
            }

            await next(context);
        }

        #endregion
    }
}
