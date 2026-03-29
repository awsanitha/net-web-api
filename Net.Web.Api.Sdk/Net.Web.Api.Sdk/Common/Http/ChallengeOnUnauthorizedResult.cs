using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ChallengeOnUnauthorizedResult.
    /// Implements the <see cref="IActionResult" />
    /// </summary>
    public class ChallengeOnUnauthorizedResult : IActionResult
    {
        #region Properties

        /// <summary>
        /// Gets the challenge.
        /// </summary>
        public AuthenticationHeaderValue Challenge { get; }

        /// <summary>
        /// Gets the inner result.
        /// </summary>
        public IActionResult InnerResult { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeOnUnauthorizedResult"/> class.
        /// </summary>
        public ChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        #endregion

        #region IActionResult Implementations

        /// <inheritdoc />
        public async Task ExecuteResultAsync(Microsoft.AspNetCore.Mvc.ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                var existing = context.HttpContext.Response.Headers["WWW-Authenticate"].ToArray();

                if (!existing.Any(h => h.StartsWith(Challenge.Scheme)))
                {
                    context.HttpContext.Response.Headers["WWW-Authenticate"] = Challenge.ToString();
                }
            }
        }

        #endregion
    }
}
