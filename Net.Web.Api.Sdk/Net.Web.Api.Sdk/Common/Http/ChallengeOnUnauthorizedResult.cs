using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ChallengeOnUnauthorizedResult.
    /// Wraps an IActionResult and adds a WWW-Authenticate challenge header if the response is 401.
    /// </summary>
    public class ChallengeOnUnauthorizedResult : IActionResult
    {
        #region Properties

        public AuthenticationHeaderValue Challenge { get; }
        public IActionResult InnerResult { get; }

        #endregion

        #region Constructors

        public ChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        #endregion

        #region IActionResult Implementation

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await InnerResult.ExecuteResultAsync(context);

            if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                if (!context.HttpContext.Response.Headers.ContainsKey("WWW-Authenticate"))
                {
                    var headerValue = string.IsNullOrEmpty(Challenge.Parameter)
                        ? Challenge.Scheme
                        : $"{Challenge.Scheme} {Challenge.Parameter}";

                    context.HttpContext.Response.Headers["WWW-Authenticate"] = headerValue;
                }
            }
        }

        #endregion
    }
}
