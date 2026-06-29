using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Middleware
{
    /// <summary>
    /// Middleware that reads the Bearer JWT token from the Authorization header,
    /// validates it and sets <see cref="HttpContext.User"/>.
    /// </summary>
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                    if (service != null)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);
                        context.User = principal;
                    }
                }
            }
            catch
            {
                // ignored – let the next middleware/filter decide on auth
            }

            await _next(context);
        }
    }
}
