using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerConsumesFilter.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), false)
                .Cast<SwaggerConsumesAttribute>()
                .SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            operation.RequestBody ??= new OpenApiRequestBody();
            operation.RequestBody.Content.Clear();
            
            foreach (var contentType in attribute.ContentTypes)
            {
                operation.RequestBody.Content[contentType] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema { Type = "object" }
                };
            }
        }

        #endregion
    }
}
