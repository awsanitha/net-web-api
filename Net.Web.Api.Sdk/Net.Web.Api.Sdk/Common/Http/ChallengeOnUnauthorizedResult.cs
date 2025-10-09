using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class AddChallengeOnUnauthorizedResult.
    /// Implements the <see cref="ActionResult" />
    /// </summary>
    /// <seealso cref="ActionResult" />
    public class ChallengeOnUnauthorizedResult : ActionResult
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
        public ActionResult InnerResult { get; }

        #endregion

        #region ActionResult Implementations

        /// <inheritdoc />
        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The context in which the result is executed.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            var response = context.HttpContext.Response;

            if (response.StatusCode != (int)HttpStatusCode.Unauthorized)
            {
                return;
            }

            var existingHeaders = response.Headers.Where(h => h.Key == "WWW-Authenticate").SelectMany(h => h.Value);
            
            if (existingHeaders.All(h => !h.StartsWith(Challenge.Scheme)))
            {
                response.Headers["WWW-Authenticate"] = Challenge.ToString();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeOnUnauthorizedResult"/> class.
        /// </summary>
        /// <param name="challenge">The challenge.</param>
        /// <param name="innerResult">The inner result.</param>
        public ChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, ActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        #endregion
    }
}
