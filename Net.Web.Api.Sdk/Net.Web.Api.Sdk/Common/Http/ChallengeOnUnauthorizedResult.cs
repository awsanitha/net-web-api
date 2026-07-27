using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        /// Gets the scheme.
        /// </summary>
        public string Scheme { get; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        public string Parameter { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeOnUnauthorizedResult"/> class.
        /// </summary>
        /// <param name="scheme">The authentication scheme.</param>
        /// <param name="parameter">The realm parameter.</param>
        public ChallengeOnUnauthorizedResult(string scheme, string parameter = null)
        {
            Scheme = scheme;
            Parameter = parameter;
        }

        #endregion

        #region IActionResult Implementations

        /// <inheritdoc />
        public System.Threading.Tasks.Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var headerValue = string.IsNullOrEmpty(Parameter) ? Scheme : $"{Scheme} {Parameter}";
            response.Headers["WWW-Authenticate"] = headerValue;
            return System.Threading.Tasks.Task.CompletedTask;
        }

        #endregion
    }
}
