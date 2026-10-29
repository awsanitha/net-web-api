using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Http;
using Net.Web.Api.Sdk.Documentation.Attributes;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Class SdkController.
    /// Implements the <see cref="ControllerBase" />
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] {
            SwaggerSdkConstants.ABOUT
        })]
    public class SdkController : ControllerBase
    {
        #region Protected Methods

        /// <summary>
        /// Returns an internal server error response carrying the exception details, as the
        /// legacy <c>ApiController.InternalServerError</c> helper did.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>IActionResult.</returns>
        protected IActionResult InternalServerError(Exception exception)
        {
            return new ResponseActionResult(HttpStatusCode.InternalServerError, new
            {
                message = exception?.Message,
                exceptionType = exception?.GetType().FullName,
                stackTrace = exception?.StackTrace
            });
        }

        /// <summary>
        /// Returns an internal server error response with no content.
        /// </summary>
        /// <returns>IActionResult.</returns>
        protected IActionResult InternalServerError()
        {
            return new ResponseActionResult(HttpStatusCode.InternalServerError);
        }

        #endregion
    }
}
