using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerConsumesFilter. Sets the consumes media types from <see cref="SwaggerConsumesAttribute"/>.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true) ?? System.Array.Empty<object>())
                .OfType<SwaggerConsumesAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            operation.RequestBody ??= new OpenApiRequestBody();
            operation.RequestBody.Content.Clear();

            foreach (var contentType in attribute.ContentTypes)
            {
                operation.RequestBody.Content[contentType] = new OpenApiMediaType();
            }
        }

        #endregion
    }
}
