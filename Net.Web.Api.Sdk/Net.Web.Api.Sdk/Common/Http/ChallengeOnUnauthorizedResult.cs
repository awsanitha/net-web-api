using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class AddChallengeOnUnauthorizedResult.
    /// Implements the <see cref="IActionResult" />
    /// </summary>
    /// <seealso cref="IActionResult" />
    public class ChallengeOnUnauthorizedResult : IActionResult
    {
        #region Properties

        /// <summary>
        /// Gets the challenge.
        /// </summary>
        /// <value>The challenge.</value>
        public AuthenticationHeaderValue Challenge { get; }

        /// <summary>
        /// Gets the inner result.
        /// </summary>
        /// <value>The inner result.</value>
        public IActionResult InnerResult { get; }

        #endregion

        #region IActionResult Implementations

        /// <inheritdoc />
        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">The context in which the result is executed.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (InnerResult != null)
            {
                await InnerResult.ExecuteResultAsync(context);
            }

            var response = context.HttpContext.Response;

            if (response.StatusCode != (int)System.Net.HttpStatusCode.Unauthorized)
            {
                return;
            }

            var existing = response.Headers.WWWAuthenticate.ToArray();

            if (existing.All(h => !h.StartsWith(Challenge.Scheme)))
            {
                response.Headers.Append("WWW-Authenticate", Challenge.ToString());
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeOnUnauthorizedResult"/> class.
        /// </summary>
        /// <param name="challenge">The challenge.</param>
        /// <param name="innerResult">The inner result.</param>
        public ChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        #endregion
    }
}
