using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Http;
using System;
using System.Net.Http.Headers;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="scheme">The scheme.</param>
        public static void ChallengeWith(this HttpResponse response, string scheme)
        {
            ChallengeWith(response, new AuthenticationHeaderValue(scheme));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="scheme">The scheme.</param>
        /// <param name="parameter">The parameter.</param>
        public static void ChallengeWith(this HttpResponse response, string scheme, string parameter)
        {
            ChallengeWith(response, new AuthenticationHeaderValue(scheme, parameter));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="challenge">The challenge.</param>
        /// <exception cref="ArgumentNullException">response</exception>
        public static void ChallengeWith(this HttpResponse response, AuthenticationHeaderValue challenge)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            response.Headers["WWW-Authenticate"] = challenge.ToString();
        }

        #endregion
    }
}
