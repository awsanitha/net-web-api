using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Sets the consumes content-type(s) for a Swagger operation from <see cref="SwaggerConsumesAttribute"/>.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttribute<SwaggerConsumesAttribute>()
                            ?? context.MethodInfo.DeclaringType?.GetCustomAttribute<SwaggerConsumesAttribute>();

            if (attribute == null)
            {
                return;
            }

            if (operation.RequestBody == null)
            {
                return;
            }

            var existingContent = operation.RequestBody.Content.Values.ToList();
            operation.RequestBody.Content.Clear();

            foreach (var contentType in attribute.ContentTypes)
            {
                operation.RequestBody.Content[contentType] =
                    existingContent.FirstOrDefault() ?? new OpenApiMediaType();
            }
        }
    }
}
