using Net.Web.Api.Sdk.Common.Http;
using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;

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
        public static void ChallengeWith(this AuthorizationFilterContext context, string scheme)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        public static void ChallengeWith(this AuthorizationFilterContext context, string scheme, string parameter)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme, parameter));
        }

        /// <summary>
        /// Challenges the with.
        /// </summary>
        public static void ChallengeWith(this AuthorizationFilterContext context, AuthenticationHeaderValue challenge)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Result = new ChallengeOnUnauthorizedResult(challenge, context.Result);
        }

        #endregion
    }
}
