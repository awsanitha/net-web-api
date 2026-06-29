using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Overrides the consumes media types from <see cref="SwaggerConsumesAttribute"/>.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), false)
                              .FirstOrDefault() as SwaggerConsumesAttribute
                    ?? context.MethodInfo.DeclaringType?
                              .GetCustomAttributes(typeof(SwaggerConsumesAttribute), false)
                              .FirstOrDefault() as SwaggerConsumesAttribute;

            if (attr == null) return;

            // Set requestBody content types
            if (operation.RequestBody == null) return;

            var types = attr.ContentTypes.ToList();
            var existingContent = operation.RequestBody.Content.ToDictionary(c => c.Key, c => c.Value);
            operation.RequestBody.Content.Clear();

            foreach (var type in types)
            {
                if (existingContent.TryGetValue(type, out var mediaType))
                    operation.RequestBody.Content[type] = mediaType;
                else
                    operation.RequestBody.Content[type] = new OpenApiMediaType();
            }
        }
    }
}
