using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

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
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes<SwaggerConsumesAttribute>(true).SingleOrDefault()
                ?? context.MethodInfo.DeclaringType?.GetCustomAttributes<SwaggerConsumesAttribute>(true).SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            // In OpenAPI 3, consumes maps to RequestBody content types
            if (operation.RequestBody?.Content != null)
            {
                var schema = operation.RequestBody.Content.Values.FirstOrDefault()?.Schema;
                operation.RequestBody.Content.Clear();

                foreach (var type in attribute.ContentTypes)
                {
                    operation.RequestBody.Content.Add(type, new OpenApiMediaType { Schema = schema });
                }
            }
        }

        #endregion
    }
}
