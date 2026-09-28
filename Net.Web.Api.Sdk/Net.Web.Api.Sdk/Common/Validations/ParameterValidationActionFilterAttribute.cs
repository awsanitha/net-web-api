using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Class ParameterValidationActionFilterAttribute.
    /// Implements the <see cref="ActionFilterAttribute" />
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                context.Result = new BadRequestObjectResult(CreateValidationErrorContent(context));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the content of the validation error.
        /// </summary>
        /// <param name="context">The action executing context.</param>
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
