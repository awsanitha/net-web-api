using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Sets the produces content-type(s) for a Swagger operation from <see cref="SwaggerProducesAttribute"/>.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttribute<SwaggerProducesAttribute>()
                            ?? context.MethodInfo.DeclaringType?.GetCustomAttribute<SwaggerProducesAttribute>();

            if (attribute == null)
            {
                return;
            }

            foreach (var response in operation.Responses.Values)
            {
                var existingContent = response.Content.Values.ToList();
                response.Content.Clear();

                foreach (var contentType in attribute.ContentTypes)
                {
                    response.Content[contentType] =
                        existingContent.FirstOrDefault() ?? new OpenApiMediaType();
                }
            }
        }
    }
}
