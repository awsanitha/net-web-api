using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ChallengeOnUnauthorizedResult.
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

        #region IActionResult Implementations

        /// <inheritdoc />
        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            if (context.HttpContext.Response.StatusCode != StatusCodes.Status401Unauthorized)
            {
                return;
            }

            var existingHeaders = context.HttpContext.Response.Headers["WWW-Authenticate"].ToArray();
            if (existingHeaders.All(h => !h.StartsWith(Challenge.Scheme)))
            {
                context.HttpContext.Response.Headers.Append("WWW-Authenticate", Challenge.ToString());
            }
        }

        #endregion
    }
}
