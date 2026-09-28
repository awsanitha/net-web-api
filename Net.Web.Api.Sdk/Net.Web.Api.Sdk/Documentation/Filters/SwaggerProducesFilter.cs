using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
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
            var attribute = context.MethodInfo.GetCustomAttributes<SwaggerProducesAttribute>(true).SingleOrDefault()
                ?? context.MethodInfo.DeclaringType?.GetCustomAttributes<SwaggerProducesAttribute>(true).SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            // In OpenAPI 3, produces maps to response content types
            foreach (var response in operation.Responses.Values)
            {
                if (response.Content == null || !response.Content.Any())
                {
                    continue;
                }

                var schema = response.Content.Values.FirstOrDefault()?.Schema;
                response.Content.Clear();

                foreach (var type in attribute.ContentTypes)
                {
                    response.Content.Add(type, new OpenApiMediaType { Schema = schema });
                }
            }
        }

        #endregion
    }
}
