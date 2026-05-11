using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ResponseActionResult.
    /// Implements the <see cref="IActionResult" />
    /// </summary>
    /// <seealso cref="IActionResult" />
    public class ResponseActionResult : IActionResult
    {
        #region Private Properties

        /// <summary>
        /// The status code
        /// </summary>
        private readonly HttpStatusCode _statusCode;

        /// <summary>
        /// The content
        /// </summary>
        private readonly object _content;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseActionResult" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        public ResponseActionResult(HttpStatusCode statusCode, object content = null)
        {
            _statusCode = statusCode;
            _content = content;
        }

        #endregion

        #region IActionResult Implementations

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>Task.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            var result = _content != null
                ? new ObjectResult(_content) { StatusCode = (int)_statusCode }
                : (IActionResult)new StatusCodeResult((int)_statusCode);

            return result.ExecuteResultAsync(context);
        }

        #endregion
    }
}
