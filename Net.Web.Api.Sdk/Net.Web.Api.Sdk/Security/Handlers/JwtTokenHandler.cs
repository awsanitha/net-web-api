using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;

namespace Net.Web.Api.Sdk.Security.Handlers
{
    /// <summary>
    /// Class JwtTokenHandler.
    /// Middleware that validates an inbound JWT bearer token (if present) and assigns the
    /// resulting <see cref="System.Security.Claims.ClaimsPrincipal"/> to the current request.
    /// </summary>
    public class JwtTokenHandler
    {
        #region Private Properties

        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenHandler"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public JwtTokenHandler(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// invoke as an asynchronous operation.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.GetToken(out var securityToken);

                if (string.IsNullOrEmpty(token) || securityToken == null)
                {
                    await _next(context);

                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);

                context.User = principal;

                Thread.CurrentPrincipal = principal;
            }
            catch
            {
                // ignored
            }

            await _next(context);
        }

        #endregion
    }
}
