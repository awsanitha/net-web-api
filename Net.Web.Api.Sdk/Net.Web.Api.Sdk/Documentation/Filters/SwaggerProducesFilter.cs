using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerProducesFilter. Sets the produces media types from <see cref="SwaggerProducesAttribute"/>.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .OfType<SwaggerProducesAttribute>()
                .SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            operation.Responses.Clear();

            foreach (var contentType in attribute.ContentTypes)
            {
                operation.Responses["200"] = new OpenApiResponse
                {
                    Content = { [contentType] = new OpenApiMediaType() }
                };
            }
        }

        #endregion
    }
}
