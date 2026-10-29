using System;
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
    /// Middleware replacement of the Web API <c>DelegatingHandler</c>: it validates the bearer token
    /// of the incoming request and publishes the resulting principal on the request context.
    /// </summary>
    public class JwtTokenHandler
    {
        #region Private Properties

        /// <summary>
        /// The next middleware in the pipeline
        /// </summary>
        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenHandler"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        public JwtTokenHandler(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
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
                var token = context.Request.GetToken(out var securityToken);

                if (!string.IsNullOrEmpty(token) && securityToken != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                    var principal = tokenHandler.ValidateToken(token, service.GetTokenValidationParameters(), out _);

                    context.User = principal;

                    Thread.CurrentPrincipal = principal;
                }
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
