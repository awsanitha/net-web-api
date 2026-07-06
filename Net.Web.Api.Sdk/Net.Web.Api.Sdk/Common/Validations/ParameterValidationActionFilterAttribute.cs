using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Class ParameterValidationActionFilterAttribute.
    /// Validates model state and returns 400 BadRequest with error details if invalid.
    /// </summary>
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
        #region ActionFilterAttribute Overrides

        /// <inheritdoc />
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

                context.Result = new ObjectResult(JArray.FromObject(errors))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        #endregion
    }
}
