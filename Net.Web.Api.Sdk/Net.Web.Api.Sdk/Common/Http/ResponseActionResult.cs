using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        /// <summary>
        /// The serializer settings
        /// </summary>
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// The default serializer settings
        /// </summary>
        private static readonly JsonSerializerSettings _defaultSerializerSettings = new JsonSerializerSettings
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
        /// <param name="serializerSettings">The serializer settings.</param>
        public ResponseActionResult(HttpStatusCode statusCode, object content = null, JsonSerializerSettings serializerSettings = null)
        {
            _statusCode = statusCode;
            _content = content;
            _serializerSettings = serializerSettings ?? _defaultSerializerSettings;
        }

        #endregion

        #region IActionResult Implementations

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)_statusCode;

            if (_content != null)
            {
                response.ContentType = "application/json";
                var json = JsonConvert.SerializeObject(_content, _serializerSettings);
                await response.WriteAsync(json);
            }
        }

        #endregion
    }
}
