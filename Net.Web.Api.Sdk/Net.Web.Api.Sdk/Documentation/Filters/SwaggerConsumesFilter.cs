using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerConsumesFilter.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .FirstOrDefault() as SwaggerConsumesAttribute
                ?? context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .FirstOrDefault() as SwaggerConsumesAttribute;

            if (attribute == null)
            {
                return;
            }

            if (operation.RequestBody == null)
            {
                operation.RequestBody = new OpenApiRequestBody();
            }

            // Clear existing content types and add specified ones
            var existingContent = operation.RequestBody.Content.ToList();
            operation.RequestBody.Content.Clear();

            foreach (var contentType in attribute.ContentTypes)
            {
                if (existingContent.Any(c => c.Key == contentType))
                {
                    var existing = existingContent.First(c => c.Key == contentType);
                    operation.RequestBody.Content.Add(existing.Key, existing.Value);
                }
                else
                {
                    operation.RequestBody.Content.Add(contentType, new OpenApiMediaType());
                }
            }
        }
    }
}
