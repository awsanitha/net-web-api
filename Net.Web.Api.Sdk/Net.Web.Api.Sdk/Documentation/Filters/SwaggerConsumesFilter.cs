using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerConsumesFilter.
    /// </summary>
    public class SwaggerConsumesFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes<SwaggerConsumesAttribute>().SingleOrDefault();

            if (attribute == null)
            {
                return;
            }

            if (operation.RequestBody?.Content != null)
            {
                var keysToRemove = operation.RequestBody.Content.Keys
                    .Where(k => !attribute.ContentTypes.Contains(k)).ToList();
                foreach (var key in keysToRemove)
                {
                    operation.RequestBody.Content.Remove(key);
                }
            }
        }

        #endregion
    }
}
