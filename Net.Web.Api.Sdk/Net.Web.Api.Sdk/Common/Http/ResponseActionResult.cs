using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The context in which the result is executed.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public System.Threading.Tasks.Task ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(_content)
            {
                StatusCode = (int)_statusCode
            };
            return objectResult.ExecuteResultAsync(context);
        }

        #endregion
    }
}
