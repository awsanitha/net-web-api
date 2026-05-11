using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .FirstOrDefault() as SwaggerProducesAttribute;

            if (attribute == null)
            {
                return;
            }

            var existingResponses = operation.Responses.ToList();

            foreach (var response in existingResponses)
            {
                var existingContent = response.Value.Content.ToList();
                response.Value.Content.Clear();

                foreach (var contentType in attribute.ContentTypes)
                {
                    if (existingContent.Any(c => c.Key == contentType))
                    {
                        var existing = existingContent.First(c => c.Key == contentType);
                        response.Value.Content.Add(existing.Key, existing.Value);
                    }
                    else
                    {
                        response.Value.Content.Add(contentType, new OpenApiMediaType());
                    }
                }
            }
        }
    }
}
