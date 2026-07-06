using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerConsumesFilter.
    /// Sets the consumes content types from the SwaggerConsumesAttribute.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), false)
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerConsumesAttribute), false) ?? Enumerable.Empty<object>())
                .Cast<SwaggerConsumesAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            operation.RequestBody ??= new OpenApiRequestBody();

            foreach (var contentType in attribute.ContentTypes)
            {
                if (!operation.RequestBody.Content.ContainsKey(contentType))
                {
                    operation.RequestBody.Content[contentType] = new OpenApiMediaType();
                }
            }

            // Remove content types not in the attribute
            var toRemove = operation.RequestBody.Content.Keys
                .Where(k => !attribute.ContentTypes.Contains(k))
                .ToList();

            foreach (var key in toRemove)
            {
                operation.RequestBody.Content.Remove(key);
            }
        }
    }
}
