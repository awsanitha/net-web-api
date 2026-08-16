using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Authorization filter that validates JWT bearer tokens.
    /// Apply to a controller or action to require a valid token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Public Properties

        /// <summary>Gets or sets the comma-separated list of valid issuers.</summary>
        public string Issuers { get; set; }

        /// <summary>Gets or sets the comma-separated list of valid intended audiences.</summary>
        public string IntendedAudiences { get; set; }

        /// <summary>Gets or sets whether token expiration is validated. Defaults to <c>true</c>.</summary>
        public bool ValidateExpiration { get; set; } = true;

        /// <summary>Gets or sets the token name to validate against.</summary>
        public string TokenValidatingName { get; set; }

        #endregion

        #region IAsyncAuthorizationFilter

        /// <inheritdoc />
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                return Task.CompletedTask;
            }

            var token = context.HttpContext.Request.GetToken(out _);

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return Task.CompletedTask;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();

            var validatingName = TokenValidatingName;
            if (string.IsNullOrEmpty(validatingName))
            {
                validatingName = claims.GetClaimByName(TokenInternalClaimNames.tn.ToString())?.Value;
            }

            if (string.IsNullOrEmpty(validatingName))
            {
                context.Result = new ObjectResult(TokenStatus.Invalid)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return Task.CompletedTask;
            }

            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
            var tokenDefinition = service?.Tokens.ContainsKey(validatingName) == true
                ? service.Tokens[validatingName]
                : null;

            if (tokenDefinition == null)
            {
                context.Result = new ObjectResult(TokenStatus.Invalid)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
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
                    context.Result = new ObjectResult(TokenStatus.Revoked)
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    };
                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    var isUsed = service.IsTokenUsed(token, claims);
                    if (isUsed)
                    {
                        context.Result = new ObjectResult(TokenStatus.AlreadyUsed)
                        {
                            StatusCode = (int)HttpStatusCode.Unauthorized
                        };
                        return Task.CompletedTask;
                    }

                    service.MarkTokenAsUsed(token, claims);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Result = new ObjectResult(TokenStatus.Expired)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                context.Result = new ObjectResult(TokenStatus.InvalidAudience)
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            catch (Exception ex)
            {
                var isTechnicalError = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));
                context.Result = isTechnicalError
                    ? new ObjectResult(ex.Message) { StatusCode = (int)HttpStatusCode.InternalServerError }
                    : new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
