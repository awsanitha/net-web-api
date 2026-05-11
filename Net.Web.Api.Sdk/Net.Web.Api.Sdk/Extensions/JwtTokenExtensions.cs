using Net.Web.Api.Sdk.Models.Token;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class JwtTokenExtensions.
    /// </summary>
    public static class JwtTokenExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Gets the token from an ActionExecutingContext.
        /// </summary>
        public static string GetToken(this ActionExecutingContext context)
        {
            return GetToken(context.HttpContext.Request, out _);
        }

        /// <summary>
        /// Gets the token from an HttpRequest.
        /// </summary>
        public static string GetToken(this HttpRequest httpRequest)
        {
            return GetToken(httpRequest, out _);
        }

        /// <summary>
        /// Gets the token from an ActionExecutingContext with security token out.
        /// </summary>
        public static string GetToken(this ActionExecutingContext context, out JwtSecurityToken securityToken)
        {
            return GetToken(context.HttpContext.Request, out securityToken);
        }

        /// <summary>
        /// Gets the token from an HttpRequest with security token out.
        /// </summary>
        public static string GetToken(this HttpRequest httpRequest, out JwtSecurityToken securityToken)
        {
            securityToken = null;
            var authHeader = httpRequest.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            var parameter = authHeader.Substring("Bearer ".Length).Trim();

            return parameter.GetToken(out securityToken);
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
        /// Determines whether the token is one time use.
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
