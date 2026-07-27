using Net.Web.Api.Sdk.Models.Token;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class JwtTokenExtensions.
    /// </summary>
    public static class JwtTokenExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Gets the token from an HttpRequest.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpRequest httpRequest)
        {
            return GetTokenFromAuthHeader(httpRequest, out _);
        }

        /// <summary>
        /// Gets the token from an HttpRequest.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>System.String.</returns>
        public static string GetToken(this HttpRequest httpRequest, out JwtSecurityToken securityToken)
        {
            return GetTokenFromAuthHeader(httpRequest, out securityToken);
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
        /// <param name="claims">The claims.</param>
        /// <returns><c>true</c> if [is token one time use] [the specified claims]; otherwise, <c>false</c>.</returns>
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

        #region Private Methods

        /// <summary>
        /// Gets the token from the Authorization header of an HttpRequest.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="securityToken">The security token.</param>
        /// <returns>System.String.</returns>
        private static string GetTokenFromAuthHeader(HttpRequest request, out JwtSecurityToken securityToken)
        {
            securityToken = null;

            var authHeader = request?.Headers?.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                return null;
            }

            if (!AuthenticationHeaderValue.TryParse(authHeader, out var headerValue))
            {
                return null;
            }

            if (string.IsNullOrEmpty(headerValue?.Scheme) || !headerValue.Scheme.Equals("Bearer"))
            {
                return null;
            }

            return headerValue.Parameter.GetToken(out securityToken);
        }

        #endregion
    }
}
