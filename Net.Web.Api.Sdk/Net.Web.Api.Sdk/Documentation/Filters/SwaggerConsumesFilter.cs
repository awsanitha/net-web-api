using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerConsumesFilter.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .FirstOrDefault() as SwaggerConsumesAttribute
                ?? context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerConsumesAttribute), true)
                .FirstOrDefault() as SwaggerConsumesAttribute;

            if (attribute == null)
            {
                return;
            }

            // Content types are handled via Consumes attribute on the request body
        }

        #endregion
    }
}
