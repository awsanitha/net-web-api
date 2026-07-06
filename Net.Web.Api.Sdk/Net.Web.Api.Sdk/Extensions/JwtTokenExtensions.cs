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
        /// Gets the bearer token from the HTTP context.
        /// </summary>
        public static string GetBearerToken(this HttpContext httpContext, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            var authHeader = httpContext?.Request?.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var rawToken = authHeader.Substring("Bearer ".Length).Trim();

            return rawToken.GetToken(out securityToken);
        }

        /// <summary>
        /// Gets the bearer token from the HTTP request.
        /// </summary>
        public static string GetBearerToken(this HttpRequest request, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            var authHeader = request?.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var rawToken = authHeader.Substring("Bearer ".Length).Trim();

            return rawToken.GetToken(out securityToken);
        }

        /// <summary>
        /// Gets the token from a raw token string.
        /// </summary>
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
        public static DateTime GetExpirationDate(this string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return DateTime.MinValue;
            }

            GetToken(token, out var jwt);

            if (jwt == null)
            {
                return DateTime.MinValue;
            }

            return jwt.ValidTo;
        }

        /// <summary>
        /// Determines whether [is token one time use] [the specified claims].
        /// </summary>
        public static bool IsTokenOneTimeUse(this List<Claim> claims)
        {
            if (claims == null || !claims.Any())
            {
                return false;
            }

            var oneTimeUseValue = claims.GetClaimByName(TokenInternalClaimNames.otu.ToString())?.Value;

            return !string.IsNullOrEmpty(oneTimeUseValue) && bool.TryParse(oneTimeUseValue, out var value) && value;
        }

        #endregion
    }
}
