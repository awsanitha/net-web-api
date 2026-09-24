using Net.Web.Api.Sdk.Common.Http;
using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// Provides extension methods for adding WWW-Authenticate challenge headers.
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

            var headerValue = string.IsNullOrEmpty(challenge.Parameter)
                ? challenge.Scheme
                : $"{challenge.Scheme} {challenge.Parameter}";
            response.Headers["WWW-Authenticate"] = Microsoft.Extensions.Primitives.StringValues.Concat(response.Headers["WWW-Authenticate"], headerValue);
        }

        #endregion
    }
}
