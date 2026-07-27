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
    /// Class SwaggerConsumesFilter.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            SwaggerConsumesAttribute attribute = null;

            if (context.MethodInfo != null)
            {
                attribute = context.MethodInfo.GetCustomAttribute<SwaggerConsumesAttribute>(false);
            }

            if (attribute == null && context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerAction)
            {
                attribute = controllerAction.ControllerTypeInfo
                    .GetCustomAttribute<SwaggerConsumesAttribute>(true);
            }

            if (attribute == null)
            {
                return;
            }

            operation.RequestBody ??= new OpenApiRequestBody();
            // Setting consumes via RequestBody content types
            foreach (var contentType in attribute.ContentTypes)
            {
                if (!operation.RequestBody.Content.ContainsKey(contentType))
                {
                    operation.RequestBody.Content[contentType] = new OpenApiMediaType();
                }
            }
        }

        #endregion
    }
}
