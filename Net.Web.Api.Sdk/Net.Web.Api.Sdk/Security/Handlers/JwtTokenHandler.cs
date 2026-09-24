using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenHandler.
    /// ASP.NET Core middleware that validates JWT tokens on incoming requests.
    /// </summary>
    public class JwtTokenHandler : IMiddleware
    {
        #region Private Properties

        /// <summary>
        /// The JWT token service
        /// </summary>
        private readonly IJwtTokenService _tokenService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenHandler"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        public JwtTokenHandler(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        #endregion

        #region IMiddleware Implementation

        /// <inheritdoc />
        /// <summary>
        /// Request handling method.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="next">The delegate representing the remaining middleware in the request pipeline.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var token = context.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(token, _tokenService.GetTokenValidationParameters(), out _);

                    context.User = principal;
                }
            }
            catch
            {
                // ignored — non-bearer or invalid requests pass through
            }

            await next(context);
        }

        #endregion
    }
}
