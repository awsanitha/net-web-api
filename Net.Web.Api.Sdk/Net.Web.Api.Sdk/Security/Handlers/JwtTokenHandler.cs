using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenMiddleware.
    /// ASP.NET Core middleware that validates JWT tokens from incoming requests.
    /// </summary>
    public class JwtTokenMiddleware
    {
        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
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
        /// Invokes the middleware asynchronously.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="jwtTokenService">The JWT token service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext httpContext, IJwtTokenService jwtTokenService)
        {
            try
            {
                var token = httpContext.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(token, jwtTokenService.GetTokenValidationParameters(), out _);
                    httpContext.User = principal;
                }
            }
            catch
            {
                // ignored - let the request continue without authentication
            }

            await _next(httpContext);
        }
    }
}
