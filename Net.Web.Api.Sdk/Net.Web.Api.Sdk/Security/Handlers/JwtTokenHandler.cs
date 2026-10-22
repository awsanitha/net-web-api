using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// ASP.NET Core middleware that authenticates JWT tokens from the Authorization header.
    /// </summary>
    public class JwtTokenHandler : IMiddleware
    {
        private readonly IJwtTokenService _tokenService;

        public JwtTokenHandler(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var authHeader = context.Request.Headers.Authorization.ToString();

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var rawToken = authHeader.Substring("Bearer ".Length).Trim();
                    var token = rawToken.GetToken(out var securityToken);

                    if (!string.IsNullOrEmpty(token) && securityToken != null)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var principal = tokenHandler.ValidateToken(token, _tokenService.GetTokenValidationParameters(), out _);

                        context.User = principal;
                    }
                }
            }
            catch
            {
                // ignored - let the request continue unauthenticated
            }

            await next(context);
        }
    }
}
