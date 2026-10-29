using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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

        /// <summary>
        /// Executes the inner result and appends the challenge header when the response is unauthorized.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            // The challenge header must be registered BEFORE the inner result writes the body:
            // once the response has started, header mutations are ignored (or throw).
            if (Challenge != null)
            {
                response.OnStarting(() =>
                {
                    if (response.StatusCode != (int)HttpStatusCode.Unauthorized)
                    {
                        return Task.CompletedTask;
                    }

                    var existing = response.Headers[HeaderNames.WWWAuthenticate];

                    foreach (var value in existing)
                    {
                        if (!string.IsNullOrEmpty(value) && value.StartsWith(Challenge.Scheme))
                        {
                            return Task.CompletedTask;
                        }
                    }

                    response.Headers.Append(HeaderNames.WWWAuthenticate, Challenge.ToString());

                    return Task.CompletedTask;
                });
            }

            if (InnerResult != null)
            {
                await InnerResult.ExecuteResultAsync(context);
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
