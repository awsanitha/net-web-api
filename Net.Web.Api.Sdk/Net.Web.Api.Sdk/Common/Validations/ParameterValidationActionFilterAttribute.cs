using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Class ParameterValidationActionFilterAttribute.
    /// Implements the <see cref="Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute" />
    public class ParameterValidationActionFilterAttribute : Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute
    {
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
