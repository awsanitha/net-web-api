using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class TokenAuthorizeAttribute.
    /// Implements the <see cref="Attribute" />
    /// Implements the <see cref="IAsyncAuthorizationFilter" />
    /// </summary>
    /// <seealso cref="Attribute" />
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
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <returns>Task.</returns>
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new JsonResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Forbidden };

                return Task.CompletedTask;
            }

            var token = context.HttpContext.GetToken();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();

            if (string.IsNullOrEmpty(TokenValidatingName))
            {
                TokenValidatingName = claims.GetClaimByName(TokenInternalClaimNames.tn.ToString())?.Value;
            }

            if (string.IsNullOrEmpty(TokenValidatingName))
            {
                context.Result = new JsonResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }

            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
            var tokenDefinition = service.Tokens.ContainsKey(TokenValidatingName) ? service.Tokens[TokenValidatingName] : null;

            if (tokenDefinition == null)
            {
                context.Result = new JsonResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }

            try
            {
                var validationParameters = service.GetTokenValidationParameters(ValidateExpiration, Issuers, IntendedAudiences);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, validationParameters, out _);

                var isRevoked = service.IsTokenRevoked(token, claims);

                if(isRevoked)
                {
                    context.Result = new JsonResult(TokenStatus.Revoked) { StatusCode = (int)HttpStatusCode.Unauthorized };

                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);

                    if (isUsed)
                    {
                        context.Result = new JsonResult(TokenStatus.AlreadyUsed) { StatusCode = (int)HttpStatusCode.Unauthorized };

                        return Task.CompletedTask;
                    }

                    service.MarkTokenAsUsed(token, claims);
                }

                return Task.CompletedTask;
            }
            catch(SecurityTokenExpiredException)
            {
                context.Result = new JsonResult(TokenStatus.Expired) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                context.Result = new JsonResult(TokenStatus.InvalidAudience) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var isTechnicalError = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));

                context.Result = isTechnicalError
                    ? new JsonResult(ex.Message) { StatusCode = (int)HttpStatusCode.InternalServerError }
                    : new JsonResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };

                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
