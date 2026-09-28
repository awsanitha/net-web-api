using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Models.Token;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class JwtTokenExtensions.
    /// </summary>
    public static class JwtTokenExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Gets the token from an HttpContext.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpContext httpContext)
        {
            return GetToken(httpContext.Request, out _);
        }

        /// <summary>
        /// Gets the token from an HttpRequest.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpRequest httpRequest)
        {
            return GetToken(httpRequest, out _);
        }

        /// <summary>
        /// Gets the token from an HttpContext.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpContext httpContext, out JwtSecurityToken securityToken)
        {
            return GetToken(httpContext.Request, out securityToken);
        }

        /// <summary>
        /// Gets the token from an HttpRequest.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpRequest httpRequest, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            var authHeader = httpRequest.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                return null;
            }

            // Parse "Bearer <token>" format
            var parts = authHeader.Split(' ', 2);
            if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return parts[1].GetToken(out securityToken);
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="rawToken">The raw token.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this string rawToken, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            if (string.IsNullOrEmpty(rawToken))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                securityToken = tokenHandler.ReadToken(rawToken) as JwtSecurityToken;

                return rawToken;
            }
            catch
            {
                try
                {
                    rawToken = rawToken.FromSecuredEncoded64Padding();
                    rawToken = Encoding.UTF8.GetString(Convert.FromBase64String(rawToken));
                    securityToken = tokenHandler.ReadToken(rawToken) as JwtSecurityToken;

                    return rawToken;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the expiration date.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>DateTime.</returns>
        public static DateTime GetExpirationDate(this string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                return DateTime.MinValue;
            }

            GetToken(token, out var jwt);

            if(jwt == null)
            {
                return DateTime.MinValue;
            }

            return jwt.ValidTo;
        }

        /// <summary>
        /// Determines whether [is token one time use] [the specified claims].
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns><c>true</c> if [is token one time use] [the specified claims]; otherwise, <c>false</c>.</returns>
        public static bool IsTokenOneTimeUse(this List<Claim> claims)
        {
            if(claims == null || !claims.Any())
            {
                return false;
            }

            var oneTimeUseValue = claims.GetClaimByName(TokenInternalClaimNames.otu.ToString())?.Value;

            return !string.IsNullOrEmpty(oneTimeUseValue) && bool.TryParse(oneTimeUseValue, out var value) && value;
        }

        #endregion
    }
}
