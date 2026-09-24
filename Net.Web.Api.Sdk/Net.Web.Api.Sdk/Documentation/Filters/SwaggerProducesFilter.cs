using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerProducesFilter.
    /// </summary>
    public class SwaggerProducesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes<SwaggerProducesAttribute>().SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            foreach (var response in operation.Responses)
            {
                if (response.Value.Content == null)
                {
                    continue;
                }

                var keysToRemove = response.Value.Content.Keys
                    .Where(k => !attribute.ContentTypes.Contains(k)).ToList();
                foreach (var key in keysToRemove)
                {
                    response.Value.Content.Remove(key);
                }
            }
        }

        #endregion
    }
}
