using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Action filter that returns a 400 Bad Request with a JSON array of error messages when model
    /// state is invalid.  Register globally via <c>AddControllers(o =&gt; o.Filters.Add(...))</c>
    /// or the SDK's <c>AddSdkWebApi</c> extension.
    /// </summary>
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ObjectResult(CreateValidationErrorContent(context))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        private static JArray CreateValidationErrorContent(ActionExecutingContext context)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .ToList();

            return JArray.FromObject(errors);
        }
    }
}
