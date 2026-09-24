using Microsoft.AspNetCore.Http;
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
using System.Security.Claims;
using System.Threading.Tasks;

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

        #region IAsyncAuthorizationFilter Implementation

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <returns>Task.</returns>
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var identity = httpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.TokenRequired) { StatusCode = StatusCodes.Status403Forbidden };
                return Task.CompletedTask;
            }

            var token = httpContext.Request.GetToken(out _);

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.TokenRequired) { StatusCode = StatusCodes.Status401Unauthorized };
                return Task.CompletedTask;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();

            if (string.IsNullOrEmpty(TokenValidatingName))
            {
                TokenValidatingName = claims.GetClaimByName(JwtTokenModel.TokenInternalClaimNames.tn.ToString())?.Value;
            }

            if (string.IsNullOrEmpty(TokenValidatingName))
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.Invalid) { StatusCode = StatusCodes.Status401Unauthorized };
                return Task.CompletedTask;
            }

            var service = httpContext.RequestServices.GetService<IJwtTokenService>();
            var tokenDefinition = service.Tokens.ContainsKey(TokenValidatingName) ? service.Tokens[TokenValidatingName] : null;

            if (tokenDefinition == null)
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.Invalid) { StatusCode = StatusCodes.Status401Unauthorized };
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
                    context.Result = new ObjectResult(JwtTokenModel.TokenStatus.Revoked) { StatusCode = StatusCodes.Status401Unauthorized };
                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);

                    if (isUsed)
                    {
                        context.Result = new ObjectResult(JwtTokenModel.TokenStatus.AlreadyUsed) { StatusCode = StatusCodes.Status401Unauthorized };
                        return Task.CompletedTask;
                    }

                    service.MarkTokenAsUsed(token, claims);
                }

                return Task.CompletedTask;
            }
            catch (SecurityTokenExpiredException)
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.Expired) { StatusCode = StatusCodes.Status401Unauthorized };
                return Task.CompletedTask;
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                context.Result = new ObjectResult(JwtTokenModel.TokenStatus.InvalidAudience) { StatusCode = StatusCodes.Status401Unauthorized };
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var isTechnicalError = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));

                context.Result = isTechnicalError
                    ? new ObjectResult(ex) { StatusCode = StatusCodes.Status500InternalServerError }
                    : new ObjectResult(JwtTokenModel.TokenStatus.Invalid) { StatusCode = StatusCodes.Status401Unauthorized };

                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
