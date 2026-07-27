using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class TokenAuthorizeAttribute.
    /// Implements the <see cref="AuthorizeAttribute" />
    /// </summary>
    /// <seealso cref="AuthorizeAttribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAuthorizationFilter
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

        #region IAuthorizationFilter Implementations

        /// <inheritdoc />
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            var token = context.HttpContext.Request.GetToken();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();

            var tokenValidatingName = TokenValidatingName;
            if (string.IsNullOrEmpty(tokenValidatingName))
            {
                tokenValidatingName = claims.GetClaimByName(TokenInternalClaimNames.tn.ToString())?.Value;
            }

            if (string.IsNullOrEmpty(tokenValidatingName))
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }

            var service = context.HttpContext.RequestServices.GetService<IJwtTokenService>();

            if (service == null)
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }

            var tokenDefinition = service.Tokens.ContainsKey(tokenValidatingName) ? service.Tokens[tokenValidatingName] : null;

            if (tokenDefinition == null)
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return;
            }

            try
            {
                var validationParameters = service.GetTokenValidationParameters(ValidateExpiration, Issuers, IntendedAudiences);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, validationParameters, out _);

                var isRevoked = service.IsTokenRevoked(token, claims);

                if (isRevoked)
                {
                    context.Result = new ObjectResult(TokenStatus.Revoked) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);

                    if (isUsed)
                    {
                        context.Result = new ObjectResult(TokenStatus.AlreadyUsed) { StatusCode = (int)HttpStatusCode.Unauthorized };
                        return;
                    }

                    service.MarkTokenAsUsed(token, claims);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Result = new ObjectResult(TokenStatus.Expired) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                context.Result = new ObjectResult(TokenStatus.InvalidAudience) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }
            catch (Exception ex)
            {
                var isTechnicalError = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));

                context.Result = isTechnicalError
                    ? new ObjectResult(ex.Message) { StatusCode = (int)HttpStatusCode.InternalServerError }
                    : new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }
        }

        #endregion
    }
}
