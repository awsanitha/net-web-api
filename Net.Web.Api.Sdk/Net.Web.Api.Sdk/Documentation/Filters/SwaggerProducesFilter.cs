using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// Sets the produces content types from the SwaggerProducesAttribute.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), false)
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerProducesAttribute), false) ?? Enumerable.Empty<object>())
                .Cast<SwaggerProducesAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            // Update each response to use the specified produces content types
            foreach (var response in operation.Responses.Values)
            {
                var toRemove = response.Content.Keys
                    .Where(k => !attribute.ContentTypes.Contains(k))
                    .ToList();

                foreach (var key in toRemove)
                {
                    response.Content.Remove(key);
                }

                foreach (var contentType in attribute.ContentTypes)
                {
                    if (!response.Content.ContainsKey(contentType))
                    {
                        response.Content[contentType] = new OpenApiMediaType();
                    }
                }
            }
        }
    }
}
