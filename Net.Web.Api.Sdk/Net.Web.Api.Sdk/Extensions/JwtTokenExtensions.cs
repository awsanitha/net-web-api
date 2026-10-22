using Net.Web.Api.Sdk.Models.Token;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        /// Gets the token from a raw token string, handling both plain JWT and Base64-encoded variants.
        /// </summary>
        /// <param name="rawToken">The raw token string.</param>
        /// <param name="securityToken">The parsed security token.</param>
        /// <returns>The raw JWT string, or null if parsing fails.</returns>
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
        /// Gets the expiration date of a JWT token string.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The expiration date, or <see cref="DateTime.MinValue"/> if unavailable.</returns>
        public static DateTime GetExpirationDate(this string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return DateTime.MinValue;
            }

            GetToken(token, out var jwt);

            return jwt?.ValidTo ?? DateTime.MinValue;
        }

        /// <summary>
        /// Determines whether the token is configured for one-time use.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns><c>true</c> if the token is one-time use; otherwise, <c>false</c>.</returns>
        public static bool IsTokenOneTimeUse(this List<Claim> claims)
        {
            if (claims == null || claims.Count == 0)
            {
                return false;
            }

            var oneTimeUseValue = claims.GetClaimByName(TokenInternalClaimNames.otu.ToString())?.Value;

            return !string.IsNullOrEmpty(oneTimeUseValue)
                && bool.TryParse(oneTimeUseValue, out var value)
                && value;
        }

        #endregion
    }
}
