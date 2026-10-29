using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Net.Web.Api.Sdk.Common.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Class ParameterValidationActionFilterAttribute.
    /// Implements the <see cref="ActionFilterAttribute" />
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
        #region Private Properties

        /// <summary>
        /// The serializer settings
        /// </summary>
        private readonly JsonSerializerSettings _serializerSettings;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValidationActionFilterAttribute"/> class.
        /// </summary>
        /// <param name="serializerSettings">The serializer settings.</param>
        public ParameterValidationActionFilterAttribute(JsonSerializerSettings serializerSettings = null)
        {
            _serializerSettings = serializerSettings ?? ResponseActionResult.DefaultSerializerSettings;
        }

        #endregion

        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="context">The action context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.BadRequest,
                    CreateValidationErrorContent(context), _serializerSettings);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the content of the validation error.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>JArray.</returns>
        private static JArray CreateValidationErrorContent(ActionExecutingContext context)
        {
            var result = (
                from value
                in context.ModelState.Values
                from message
                in value.Errors
                select message.ErrorMessage).Where(c => !string.IsNullOrEmpty(c));

            return JArray.FromObject(result.Distinct().ToList());
        }

        #endregion
    }
}
