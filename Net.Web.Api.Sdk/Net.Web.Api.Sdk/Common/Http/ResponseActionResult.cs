using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ResponseActionResult.
    /// Implements the <see cref="IActionResult" />
    /// </summary>
    public class ResponseActionResult : IActionResult
    {
        #region Private Properties

        private readonly HttpStatusCode _statusCode;
        private readonly object _content;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseActionResult"/> class.
        /// </summary>
        public ResponseActionResult(HttpStatusCode statusCode, object content = null)
        {
            _statusCode = statusCode;
            _content = content;
        }

        #endregion

        #region IActionResult Implementation

        public Task ExecuteResultAsync(ActionContext context)
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
