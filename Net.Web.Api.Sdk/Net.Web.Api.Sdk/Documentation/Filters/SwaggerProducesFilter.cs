using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
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
            var attribute = context.MethodInfo?.GetCustomAttributes<SwaggerProducesAttribute>(true)
                .FirstOrDefault() ?? context.MethodInfo?.DeclaringType?
                .GetCustomAttributes<SwaggerProducesAttribute>(true).FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            // Clear the content types for each response
            foreach (var response in operation.Responses.Values)
            {
                if (response.Content == null)
                {
                    response.Content = new Dictionary<string, OpenApiMediaType>();
                }
                else
                {
                    response.Content.Clear();
                }

                // Add the new content types
                foreach (var contentType in attribute.ContentTypes)
                {
                    response.Content[contentType] = new OpenApiMediaType();
                }
            }
        }

        #endregion
    }
}