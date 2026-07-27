using Net.Web.Api.Sdk.Common.Http;
using System;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class HttpAuthenticationChallengeContextExtensions.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Creates a challenge action result for the given scheme.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>ChallengeOnUnauthorizedResult.</returns>
        public static ChallengeOnUnauthorizedResult CreateChallenge(string scheme)
        {
            return new ChallengeOnUnauthorizedResult(scheme);
        }

        /// <summary>
        /// Creates a challenge action result for the given scheme and parameter.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>ChallengeOnUnauthorizedResult.</returns>
        public static ChallengeOnUnauthorizedResult CreateChallenge(string scheme, string parameter)
        {
            return new ChallengeOnUnauthorizedResult(scheme, parameter);
        }

        #endregion
    }
}
