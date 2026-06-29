using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Overrides the produces media types from <see cref="SwaggerProducesAttribute"/>.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), false)
                              .FirstOrDefault() as SwaggerProducesAttribute;

            if (attr == null) return;

            foreach (var response in operation.Responses.Values)
            {
                var types = attr.ContentTypes.ToList();
                var existing = response.Content.ToDictionary(c => c.Key, c => c.Value);
                response.Content.Clear();
                foreach (var type in types)
                {
                    response.Content[type] = existing.TryGetValue(type, out var m) ? m : new OpenApiMediaType();
                }
            }
        }
    }
}
