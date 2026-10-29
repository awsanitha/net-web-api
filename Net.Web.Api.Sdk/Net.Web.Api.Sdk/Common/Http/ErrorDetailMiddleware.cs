using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ErrorDetailMiddleware.
    /// Replacement of the Web API <c>HttpConfiguration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always</c>
    /// setting: an unhandled exception is returned to the caller with its full detail
    /// (message, exception type, stack trace and inner exceptions), in every environment,
    /// using the same <c>HttpError</c> response shape Web API produced.
    /// </summary>
    public class ErrorDetailMiddleware
    {
        #region Private Constants

        /// <summary>
        /// The generic message Web API used as the <c>Message</c> member of an error response.
        /// </summary>
        private const string ERROR_MESSAGE = "An error has occurred.";

        #endregion

        #region Private Fields

        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The serializer settings used for the error payload.
        /// </summary>
        private readonly JsonSerializerSettings _serializerSettings;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDetailMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="serializerSettings">The serializer settings, the SDK defaults when not supplied.</param>
        public ErrorDetailMiddleware(RequestDelegate next, JsonSerializerSettings serializerSettings = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _serializerSettings = serializerSettings ?? ResponseActionResult.DefaultSerializerSettings;
        }

        #endregion

        #region Middleware Implementations

        /// <summary>
        /// Invokes the middleware, returning the full detail of an unhandled exception.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that represents the asynchronous invoke operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    // The response is already on the wire, the detail cannot be written anymore.
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = JsonConvert.SerializeObject(BuildError(exception), _serializerSettings);

                await context.Response.WriteAsync(payload);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds the Web API style error detail for the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The error detail.</returns>
        private static object BuildError(Exception exception)
        {
            return new
            {
                Message = ERROR_MESSAGE,
                ExceptionMessage = exception.Message,
                ExceptionType = exception.GetType().FullName,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException == null ? null : BuildError(exception.InnerException)
            };
        }

        #endregion
    }
}
