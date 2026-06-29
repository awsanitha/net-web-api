using System;
using System.Collections.Generic;
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
    /// Authorization filter that validates JWT tokens.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string Issuers { get; set; }
        public string IntendedAudiences { get; set; }
        public bool ValidateExpiration { get; set; }
        public string TokenValidatingName { get; set; }

        public TokenAuthorizeAttribute() => ValidateExpiration = true;

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Forbidden };
                return Task.CompletedTask;
            }

            var token = context.HttpContext.Request.GetToken(out _);

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(TokenStatus.TokenRequired) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return Task.CompletedTask;
            }

            var claims = ((ClaimsIdentity)identity).Claims.ToList();
            var tokenName = TokenValidatingName ?? claims.GetClaimByName(TokenInternalClaimNames.tn.ToString())?.Value;

            if (string.IsNullOrEmpty(tokenName))
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return Task.CompletedTask;
            }

            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
            if (service == null || !service.Tokens.ContainsKey(tokenName))
            {
                context.Result = new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
                return Task.CompletedTask;
            }

            try
            {
                var validationParameters = service.GetTokenValidationParameters(ValidateExpiration, Issuers, IntendedAudiences);
                new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);

                if (service.IsTokenRevoked(token, claims))
                {
                    context.Result = new ObjectResult(TokenStatus.Revoked) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    return Task.CompletedTask;
                }

                if (claims.IsTokenOneTimeUse())
                {
                    if (service.IsTokenUsed(token, claims))
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
                var isTechnical = !(ex.Message.StartsWith("IDX") && ex.Message.Contains(":"));
                context.Result = isTechnical
                    ? new ObjectResult(ex.Message) { StatusCode = (int)HttpStatusCode.InternalServerError }
                    : new ObjectResult(TokenStatus.Invalid) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }

            return Task.CompletedTask;
        }
    }
}
