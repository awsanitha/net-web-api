using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Adds a WWW-Authenticate challenge header to the response.
        /// </summary>
        public static void ChallengeWith(this HttpResponse response, string scheme)
        {
            response.Headers["WWW-Authenticate"] = scheme;
        }

        /// <summary>
        /// Adds a WWW-Authenticate challenge header with parameter to the response.
        /// </summary>
        public static void ChallengeWith(this HttpResponse response, string scheme, string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                response.Headers["WWW-Authenticate"] = scheme;
            }
            else
            {
                response.Headers["WWW-Authenticate"] = $"{scheme} {parameter}";
            }
        }

        #endregion
    }
}
