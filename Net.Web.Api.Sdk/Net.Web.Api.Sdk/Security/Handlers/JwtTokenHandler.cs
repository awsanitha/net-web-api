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
    /// Class JwtTokenHandler.
    /// ASP.NET Core middleware that validates JWT Bearer tokens and sets the HTTP context user principal.
    /// </summary>
    public class JwtTokenHandler
    {
        private readonly RequestDelegate _next;

        public JwtTokenHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.GetBearerToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var service = InjectionContainer.Instance.GetService<IJwtTokenService>();

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
                // Ignore validation errors - the authorization filters will handle them
            }

            await _next(context);
        }
    }
}
