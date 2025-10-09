using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Common.Http
{
    /// <summary>
    /// Class ResponseActionResult.
    /// Implements the <see cref="ActionResult" />
    /// </summary>
    /// <seealso cref="ActionResult" />
    public class ResponseActionResult : ActionResult
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

        /// <summary>
        /// The JSON settings
        /// </summary>
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// The default JSON settings
        /// </summary>
        private static readonly JsonSerializerSettings _defaultJsonSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseActionResult" /> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        /// <param name="jsonSettings">The JSON settings.</param>
        public ResponseActionResult(HttpStatusCode statusCode, object content = null, JsonSerializerSettings jsonSettings = null)
        {
            _statusCode = statusCode;
            _content = content;
            _jsonSettings = jsonSettings ?? _defaultJsonSettings;
        }

        #endregion

        #region ActionResult Implementations

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The context in which the result is executed.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)_statusCode;

            if (_content != null)
            {
                response.ContentType = "application/json";
                var json = JsonConvert.SerializeObject(_content, _jsonSettings);
                await response.WriteAsync(json);
            }
        }

        #endregion
    }
}
