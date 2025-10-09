using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Injection.Containers;
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
    /// Implements the <see cref="Attribute" />
    /// Implements the <see cref="IAuthorizationFilter" />
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="IAuthorizationFilter" />
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

        #region IAuthorizationFilter Implementation

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var request = context.HttpContext.Request;
                var token = ExtractTokenFromRequest(request);

                if (string.IsNullOrEmpty(token))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var jwtTokenService = Net.Web.Api.Sdk.Injection.Containers.WindsorContainer.Instance.Resolve<IJwtTokenService>();
                var validationResult = jwtTokenService.ValidateToken(token, new JwtTokenValidationRequest
                {
                    Issuers = Issuers?.Split(','),
                    IntendedAudiences = IntendedAudiences?.Split(','),
                    ValidateExpiration = ValidateExpiration,
                    TokenValidatingName = TokenValidatingName
                });

                if (!validationResult.IsValid)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Set the user principal
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
                context.HttpContext.User = new ClaimsPrincipal(identity);
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedResult();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts the token from the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.String.</returns>
        private static string ExtractTokenFromRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // Check query string
            if (request.Query.ContainsKey("access_token"))
            {
                return request.Query["access_token"];
            }

            return null;
        }

        #endregion
    }
}
