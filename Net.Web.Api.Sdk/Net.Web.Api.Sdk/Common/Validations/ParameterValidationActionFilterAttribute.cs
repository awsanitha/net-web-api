using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Class ParameterValidationActionFilterAttribute.
    /// Implements the <see cref="ActionFilterAttribute" />
    /// </summary>
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Result = new BadRequestObjectResult(CreateValidationErrorContent(actionContext));
            }
        }

        #endregion

        #region Private Methods

        private static JArray CreateValidationErrorContent(ActionExecutingContext actionContext)
        {
            var result = (
                from value
                in actionContext.ModelState.Values
                from message
                in value.Errors
                select message.ErrorMessage).Where(c => !string.IsNullOrEmpty(c));

            return JArray.FromObject(result.Distinct().ToList());
        }

        #endregion
    }
}
