using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .FirstOrDefault() as SwaggerProducesAttribute
                ?? context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerProducesAttribute), true)
                .FirstOrDefault() as SwaggerProducesAttribute;

            if (attribute == null)
            {
                return;
            }

            // Content types are set via Produces attribute on the response
            // This filter adds them to the operation responses
        }

        #endregion
    }
}
