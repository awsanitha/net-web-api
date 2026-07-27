using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            SwaggerProducesAttribute attribute = null;

            if (context.MethodInfo != null)
            {
                attribute = context.MethodInfo.GetCustomAttribute<SwaggerProducesAttribute>(false);
            }

            if (attribute == null && context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerAction)
            {
                attribute = controllerAction.ControllerTypeInfo
                    .GetCustomAttribute<SwaggerProducesAttribute>(true);
            }

            if (attribute == null)
            {
                return;
            }

            // Update responses to reflect produces content type
            foreach (var response in operation.Responses.Values)
            {
                var existingContent = response.Content.Keys.ToList();
                foreach (var key in existingContent)
                {
                    response.Content.Remove(key);
                }
                foreach (var contentType in attribute.ContentTypes)
                {
                    response.Content[contentType] = new OpenApiMediaType();
                }
            }
        }

        #endregion
    }
}
