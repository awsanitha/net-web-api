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
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            var response = context.HttpContext.Response;

            if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                var existingValues = response.Headers["WWW-Authenticate"].ToArray();
                if (!existingValues.Any(h => h != null && h.StartsWith(Challenge.Scheme)))
                {
                    var headerValue = string.IsNullOrEmpty(Challenge.Parameter)
                        ? Challenge.Scheme
                        : $"{Challenge.Scheme} {Challenge.Parameter}";
                    response.Headers["WWW-Authenticate"] = Microsoft.Extensions.Primitives.StringValues.Concat(
                        response.Headers["WWW-Authenticate"], headerValue);
                }
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
