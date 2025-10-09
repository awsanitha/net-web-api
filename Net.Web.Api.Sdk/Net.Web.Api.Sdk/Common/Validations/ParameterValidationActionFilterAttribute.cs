using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;

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
        /// The JSON settings
        /// </summary>
        private readonly JsonSerializerSettings _jsonSettings;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValidationActionFilterAttribute"/> class.
        /// </summary>
        /// <param name="jsonSettings">The JSON settings.</param>
        public ParameterValidationActionFilterAttribute(JsonSerializerSettings jsonSettings = null)
        {
            _jsonSettings = jsonSettings ?? new JsonSerializerSettings();
        }

        #endregion

        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationErrors = CreateValidationErrorContent(context);
                context.Result = new BadRequestObjectResult(validationErrors);
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
                from error 
                in value.Errors 
                select error.ErrorMessage).Where(c => !string.IsNullOrEmpty(c));

            return JArray.FromObject(result.Distinct().ToList());
        }

        #endregion
    }
}
