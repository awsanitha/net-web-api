using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Net;
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
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                var existing = context.HttpContext.Response.Headers["WWW-Authenticate"].ToString();

                if (!existing.Contains(Challenge.Scheme))
                {
                    var value = Challenge.Parameter != null
                        ? $"{Challenge.Scheme} {Challenge.Parameter}"
                        : Challenge.Scheme;

                    context.HttpContext.Response.Headers["WWW-Authenticate"] = value;
                }
            }
        }

        #endregion
    }
}
