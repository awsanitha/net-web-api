using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Operation filter that overrides the <c>consumes</c> (request media type) list using
    /// <see cref="SwaggerConsumesAttribute"/>.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo
                ?.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .OfType<SwaggerConsumesAttribute>()
                .SingleOrDefault();

            if (attribute == null) return;

            if (operation.RequestBody == null)
                return;

            var existingSchema = operation.RequestBody.Content.Values.FirstOrDefault()?.Schema;
            operation.RequestBody.Content.Clear();

            foreach (var ct in attribute.ContentTypes)
            {
                operation.RequestBody.Content[ct] = new OpenApiMediaType
                {
                    Schema = existingSchema
                };
            }
        }
    }
}
