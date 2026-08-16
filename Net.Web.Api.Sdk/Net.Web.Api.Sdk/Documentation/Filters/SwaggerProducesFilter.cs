using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Operation filter that overrides the <c>produces</c> (response media type) list using
    /// <see cref="SwaggerProducesAttribute"/>.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo
                ?.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .OfType<SwaggerProducesAttribute>()
                .SingleOrDefault();

            if (attribute == null) return;

            // Re-build response media types for each response entry
            foreach (var response in operation.Responses.Values)
            {
                var existingSchema = response.Content.Values.FirstOrDefault()?.Schema;

                response.Content.Clear();

                foreach (var ct in attribute.ContentTypes)
                {
                    response.Content[ct] = new OpenApiMediaType
                    {
                        Schema = existingSchema
                    };
                }
            }
        }
    }
}
