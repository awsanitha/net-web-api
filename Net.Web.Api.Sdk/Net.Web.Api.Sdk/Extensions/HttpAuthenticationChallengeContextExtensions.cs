using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Net.Web.Api.Sdk.Common.Http;
using System;
using System.Net.Http.Headers;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// Provides extension methods for setting WWW-Authenticate challenge headers on AuthorizationFilterContext.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <param name="scheme">The scheme.</param>
        public static void ChallengeWith(this AuthorizationFilterContext context, string scheme)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <param name="scheme">The scheme.</param>
        /// <param name="parameter">The parameter.</param>
        public static void ChallengeWith(this AuthorizationFilterContext context, string scheme, string parameter)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme, parameter));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <param name="challenge">The challenge.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public static void ChallengeWith(this AuthorizationFilterContext context, AuthenticationHeaderValue challenge)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Result = new ChallengeOnUnauthorizedResult(challenge, context.Result ?? new StatusCodeResult(StatusCodes.Status401Unauthorized));
        }

        #endregion
    }
}
