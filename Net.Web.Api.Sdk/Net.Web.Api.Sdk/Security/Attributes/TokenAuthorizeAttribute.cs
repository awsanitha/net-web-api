using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Common.Http;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class TokenAuthorizeAttribute.
    /// Implements the <see cref="IAsyncAuthorizationFilter" />
    /// </summary>
    /// <seealso cref="IAsyncAuthorizationFilter" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the issuers.
        /// </summary>
        /// <value>The issuers.</value>
        public string Issuers { get; set; }

        /// <summary>
        /// Gets or sets the intended audiences.
        /// </summary>
        /// <value>The intended audiences.</value>
        public string IntendedAudiences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [validate expiration].
        /// </summary>
        /// <value><c>true</c> if [validate expiration]; otherwise, <c>false</c>.</value>
        public bool ValidateExpiration { get; set; }

        /// <summary>
        /// Gets or sets the name of the token validating.
        /// </summary>
        /// <value>The name of the token validating.</value>
        public string TokenValidatingName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAuthorizeAttribute"/> class.
        /// </summary>
        public TokenAuthorizeAttribute() => ValidateExpiration = true;

        #endregion

        #region IAsyncAuthorizationFilter Implementations

        /// <summary>
        /// Called when [authorization asynchronous].
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <returns>Task.</returns>
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Forbidden, TokenStatus.TokenRequired);

                return Task.CompletedTask;
            }

            var token = context.HttpContext.Request.GetToken();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.TokenRequired);

                return Task.CompletedTask;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();
            var tokenValidatingName = TokenValidatingName;

            if (string.IsNullOrEmpty(tokenValidatingName))
            {
                tokenValidatingName = claims.GetClaimByName(TokenInternalClaimNames.tn.ToString())?.Value;
            }

            if (string.IsNullOrEmpty(tokenValidatingName))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.Invalid);

                return Task.CompletedTask;
            }

            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
            var tokenDefinition = service.Tokens.ContainsKey(tokenValidatingName) ? service.Tokens[tokenValidatingName] : null;

            if (tokenDefinition == null)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.Invalid);

                return Task.CompletedTask;
            }

            try
            {
                var validationParameters = service.GetTokenValidationParameters(ValidateExpiration, Issuers, IntendedAudiences);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, validationParameters, out _);

                var isRevoked = service.IsTokenRevoked(token, claims);

                if (isRevoked)
                {
                    context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.Revoked);

                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);

                    if (isUsed)
                    {
                        context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.AlreadyUsed);

                        return Task.CompletedTask;
                    }

                    service.MarkTokenAsUsed(token, claims);
                }

                return Task.CompletedTask;
            }
            catch (SecurityTokenExpiredException)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.Expired);

                return Task.CompletedTask;
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.InvalidAudience);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var isTechnicalError = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));

                context.Result = isTechnicalError
                    ? new ResponseActionResult(HttpStatusCode.InternalServerError, new { message = ex.Message })
                    : new ResponseActionResult(HttpStatusCode.Unauthorized, TokenStatus.Invalid);

                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
