using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    public class SwaggerProducesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .Cast<SwaggerProducesAttribute>()
                .SingleOrDefault();

            if (attribute == null)
            {
                attribute = context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                    .Cast<SwaggerProducesAttribute>()
                    .SingleOrDefault();
            }

            if (attribute == null || !attribute.ContentTypes.Any())
                return;

            foreach (var response in operation.Responses.Values)
            {
                var existingKeys = response.Content.Keys.ToList();

                foreach (var key in existingKeys)
                {
                    if (!attribute.ContentTypes.Contains(key))
                        response.Content.Remove(key);
                }
            }
        }
    }
}
