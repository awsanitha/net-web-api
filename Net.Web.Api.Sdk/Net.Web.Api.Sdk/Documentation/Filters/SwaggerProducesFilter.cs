using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(SwaggerProducesAttribute), false)
                .Cast<SwaggerProducesAttribute>()
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
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [contentType] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema { Type = "object" }
                        }
                    }
                };
            }
        }

        #endregion
    }
}
