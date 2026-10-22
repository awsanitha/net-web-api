using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="context">The action context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .ToList();

                context.Result = new BadRequestObjectResult(JArray.FromObject(errors));
            }
        }

        #endregion
    }
}
