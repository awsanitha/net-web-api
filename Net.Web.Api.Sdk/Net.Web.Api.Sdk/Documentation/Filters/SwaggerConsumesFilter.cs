using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            var attribute = SwaggerFilterHelper.GetControllerAndActionAttributes<SwaggerConsumesAttribute>(context).SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            var contentTypes = attribute.ContentTypes?.ToList();

            if (contentTypes == null || contentTypes.Count == 0)
            {
                return;
            }

            var mediaType = operation.RequestBody?.Content?.Values.FirstOrDefault();
            var content = new Dictionary<string, OpenApiMediaType>();

            foreach (var contentType in contentTypes)
            {
                content[contentType] = mediaType ?? new OpenApiMediaType();
            }

            if (operation.RequestBody == null)
            {
                operation.RequestBody = new OpenApiRequestBody();
            }

            operation.RequestBody.Content = content;
        }

        #endregion
    }
}
