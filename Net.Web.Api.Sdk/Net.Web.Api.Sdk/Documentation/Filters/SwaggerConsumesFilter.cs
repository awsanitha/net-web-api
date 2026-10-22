using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    public class SwaggerConsumesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .Cast<SwaggerConsumesAttribute>()
                .SingleOrDefault();

            if (attribute == null)
            {
                attribute = context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                    .Cast<SwaggerConsumesAttribute>()
                    .SingleOrDefault();
            }

            if (attribute == null || !attribute.ContentTypes.Any())
                return;

            if (operation.RequestBody == null)
                return;

            var existingKeys = operation.RequestBody.Content.Keys.ToList();

            foreach (var key in existingKeys)
            {
                if (!attribute.ContentTypes.Contains(key))
                    operation.RequestBody.Content.Remove(key);
            }
        }
    }
}
