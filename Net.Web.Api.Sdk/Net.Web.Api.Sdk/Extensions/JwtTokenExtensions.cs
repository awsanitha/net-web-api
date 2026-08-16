using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Extension methods for reading and parsing JWT tokens from HTTP requests.
    /// </summary>
    public static class JwtTokenExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Reads the Bearer token from the current HTTP request's Authorization header.
        /// </summary>
        public static string GetToken(this HttpRequest request)
        {
            return GetToken(request, out _);
        }

        /// <summary>
        /// Reads the Bearer token from the current HTTP request's Authorization header,
        /// and returns the parsed <see cref="JwtSecurityToken"/>.
        /// </summary>
        public static string GetToken(this HttpRequest request, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            var authHeader = request?.Headers?["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var rawToken = authHeader.Substring("Bearer ".Length).Trim();
            return rawToken.GetToken(out securityToken);
        }

        /// <summary>
        /// Parses a raw token string (plain JWT or Base64-encoded JWT).
        /// </summary>
        public static string GetToken(this string rawToken, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            if (string.IsNullOrEmpty(rawToken)) return null;

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
        /// Returns the expiration date of the token, or <see cref="DateTime.MinValue"/> on failure.
        /// </summary>
        public static DateTime GetExpirationDate(this string token)
        {
            if (string.IsNullOrEmpty(token)) return DateTime.MinValue;

            token.GetToken(out var jwt);
            return jwt?.ValidTo ?? DateTime.MinValue;
        }

        /// <summary>
        /// Returns <c>true</c> when the token claims indicate a one-time-use token.
        /// </summary>
        public static bool IsTokenOneTimeUse(this List<Claim> claims)
        {
            if (claims == null || !claims.Any()) return false;

            var value = claims.GetClaimByName(TokenInternalClaimNames.otu.ToString())?.Value;
            return !string.IsNullOrEmpty(value) && bool.TryParse(value, out var parsed) && parsed;
        }

        #endregion
    }
}
