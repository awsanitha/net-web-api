using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;
using System.IdentityModel.Tokens.Jwt;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class TokenAuthorizeAttribute. Authorization filter for JWT token validation in ASP.NET Core.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the issuers.
        /// </summary>
        public string? Issuers { get; set; }

        /// <summary>
        /// Gets or sets the intended audiences.
        /// </summary>
        public string? IntendedAudiences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [validate expiration].
        /// </summary>
        public bool ValidateExpiration { get; set; }

        /// <summary>
        /// Gets or sets the name of the token validating.
        /// </summary>
        public string? TokenValidatingName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAuthorizeAttribute"/> class.
        /// </summary>
        public TokenAuthorizeAttribute() => ValidateExpiration = true;

        #endregion

        #region IAsyncAuthorizationFilter Implementation

        /// <inheritdoc />
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var identity = httpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Forbidden };
                return Task.CompletedTask;
            }

            var authHeader = httpContext.Request.Headers.Authorization.ToString();
            string? token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var rawToken = authHeader.Substring("Bearer ".Length).Trim();
                token = rawToken.GetToken(out _);
            }

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Unauthorized };
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
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return Task.CompletedTask;
            }

            var service = httpContext.RequestServices.GetRequiredService<IJwtTokenService>();
            var tokenDefinition = service.Tokens.ContainsKey(tokenValidatingName) ? service.Tokens[tokenValidatingName] : null;

            if (tokenDefinition == null)
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
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
                    context.Result = new ObjectResult(TokenStatus.Revoked) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);

                    if (isUsed)
                    {
                        context.Result = new ObjectResult(TokenStatus.AlreadyUsed) { StatusCode = (int)HttpStatusCode.Unauthorized };
                        return Task.CompletedTask;
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

                if (isTechnicalError)
                {
                    context.Result = new ObjectResult(ex.Message) { StatusCode = (int)HttpStatusCode.InternalServerError };
                }
                else
                {
                    context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                }
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
