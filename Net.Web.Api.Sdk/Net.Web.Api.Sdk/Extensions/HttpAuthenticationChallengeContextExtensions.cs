using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http.Headers;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// Provides helper methods for adding WWW-Authenticate challenge headers to ASP.NET Core responses.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Adds a WWW-Authenticate challenge header to the response.
        /// </summary>
        public static void ChallengeWith(this HttpResponse response, string scheme)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            response.Headers["WWW-Authenticate"] = scheme;
        }

        /// <summary>
        /// Adds a WWW-Authenticate challenge header with a parameter to the response.
        /// </summary>
        public static void ChallengeWith(this HttpResponse response, string scheme, string parameter)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            response.Headers["WWW-Authenticate"] = string.IsNullOrEmpty(parameter)
                ? scheme
                : $"{scheme} {parameter}";
        }

        #endregion
    }
}
