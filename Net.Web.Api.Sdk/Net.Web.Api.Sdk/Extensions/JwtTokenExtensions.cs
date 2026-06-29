using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Extension methods for JWT token handling.
    /// </summary>
    public static class JwtTokenExtensions
    {
        /// <summary>
        /// Reads the Bearer token from an <see cref="HttpRequest"/>.
        /// </summary>
        public static string GetToken(this HttpRequest request, out JwtSecurityToken securityToken)
        {
            securityToken = null;
            var authHeader = request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring("Bearer ".Length).Trim().GetToken(out securityToken);
        }

        /// <summary>
        /// Parses a raw JWT string.
        /// </summary>
        public static string GetToken(this string rawToken, out JwtSecurityToken securityToken)
        {
            securityToken = null;
            if (string.IsNullOrEmpty(rawToken)) return null;

            var handler = new JwtSecurityTokenHandler();
            try
            {
                securityToken = handler.ReadToken(rawToken) as JwtSecurityToken;
                return rawToken;
            }
            catch
            {
                try
                {
                    rawToken = rawToken.FromSecuredEncoded64Padding();
                    rawToken = Encoding.UTF8.GetString(Convert.FromBase64String(rawToken));
                    securityToken = handler.ReadToken(rawToken) as JwtSecurityToken;
                    return rawToken;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// Gets the expiration date of a raw JWT string.
        /// </summary>
        public static DateTime GetExpirationDate(this string token)
        {
            if (string.IsNullOrEmpty(token)) return DateTime.MinValue;
            token.GetToken(out var jwt);
            return jwt?.ValidTo ?? DateTime.MinValue;
        }

        /// <summary>
        /// Returns true if the token claims indicate one-time use.
        /// </summary>
        public static bool IsTokenOneTimeUse(this List<Claim> claims)
        {
            if (claims == null || !claims.Any()) return false;
            var val = claims.GetClaimByName(TokenInternalClaimNames.otu.ToString())?.Value;
            return !string.IsNullOrEmpty(val) && bool.TryParse(val, out var b) && b;
        }
    }
}
