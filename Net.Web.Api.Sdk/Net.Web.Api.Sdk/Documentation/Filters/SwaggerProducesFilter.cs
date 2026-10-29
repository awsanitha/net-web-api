using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            var attribute = SwaggerFilterHelper.GetControllerAndActionAttributes<SwaggerProducesAttribute>(context).SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            var contentTypes = attribute.ContentTypes?.ToList();

            if (contentTypes == null || contentTypes.Count == 0 || operation.Responses == null)
            {
                return;
            }

            foreach (var response in operation.Responses.Values)
            {
                var mediaType = response.Content?.Values.FirstOrDefault();

                if (mediaType == null)
                {
                    continue;
                }

                var content = new Dictionary<string, OpenApiMediaType>();

                foreach (var contentType in contentTypes)
                {
                    content[contentType] = mediaType;
                }

                response.Content = content;
            }
        }

        #endregion
    }
}
