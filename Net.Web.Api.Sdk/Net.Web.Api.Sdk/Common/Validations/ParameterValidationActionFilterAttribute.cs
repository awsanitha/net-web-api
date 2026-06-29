using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Common.Validations
{
    /// <summary>
    /// Action filter that returns 400 Bad Request when model state is invalid.
    /// </summary>
    public class ParameterValidationActionFilterAttribute : ActionFilterAttribute
    {
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

                context.Result = new BadRequestObjectResult(errors);
            }
        }
    }
}
