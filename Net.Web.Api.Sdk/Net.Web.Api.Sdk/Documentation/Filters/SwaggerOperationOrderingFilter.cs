using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerOperationOrderingFilter.
    /// Implements the <see cref="SwaggerOrderingFilter" />
    /// </summary>
    /// <seealso cref="SwaggerOrderingFilter" />
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        #region IDocumentFilter Implementations

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return;
            }

            var allOperationNames = GetOperationOrder(swaggerDoc, context);

            if (allOperationNames == null || allOperationNames.Length == 0)
            {
                OrderingApply(swaggerDoc, context);
                return;
            }

            var groups = new Dictionary<int, IDictionary<string, OpenApiPathItem>>();

            foreach (var path in paths)
            {
                GetInvokeMethod(path.Value, out var tag);

                var index = GetOperationIndex(allOperationNames, tag);

                if (!groups.ContainsKey(index))
                {
                    groups.Add(index, new Dictionary<string, OpenApiPathItem>());
                }

                groups[index].Add(path.Key, path.Value);
            }

            var orderedPaths = new Dictionary<string, OpenApiPathItem>();

            foreach (var group in groups.OrderBy(c => c.Key))
            {
                var orderedGroup = OrderPathsByOperationOrder(group.Value, context);

                foreach (var path in orderedGroup)
                {
                    orderedPaths.Add(path.Key, path.Value);
                }
            }

            swaggerDoc.Paths.Clear();
            foreach (var path in orderedPaths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the operation order.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.String[].</returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // This would need to be implemented based on your specific ordering requirements
            // For now, returning null to fall back to default ordering
            return null;
        }

        /// <summary>
        /// Gets the operation index.
        /// </summary>
        /// <param name="allOperationNames">All operation names.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>System.Int32.</returns>
        private static int GetOperationIndex(string[] allOperationNames, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return allOperationNames.Length;
            }

            for (var i = 0; i < allOperationNames.Length; i++)
            {
                if (allOperationNames[i] == tag)
                {
                    return i;
                }
            }

            return allOperationNames.Length;
        }

        /// <summary>
        /// Orders the paths by operation order.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <param name="context">The context.</param>
        /// <returns>IDictionary&lt;System.String, OpenApiPathItem&gt;.</returns>
        private static IDictionary<string, OpenApiPathItem> OrderPathsByOperationOrder(
            IDictionary<string, OpenApiPathItem> paths, DocumentFilterContext context)
        {
            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in paths)
            {
                var key = GetInvokeMethod(path.Value, out var tag);
                var apiKey = $"{key}{path.Key.TrimStart('/')}";
                var apiFound = context.ApiDescriptions.FirstOrDefault(c => c.RelativePath?.StartsWith(apiKey.TrimStart('/')) == true);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
                }

                var item = new ApiOrder
                {
                    Order = GetApiOrder(apiFound),
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                };

                tagGroups[tag].Add(item);
                tagGroups[tag] = tagGroups[tag].OrderBy(c => c.Order).ToList();
            }

            var list = new List<ApiOrder>();

            foreach (var tagGroup in tagGroups)
            {
                list.AddRange(tagGroup.Value);
            }

            return list.OrderBy(c => c.OperationName).ToDictionary(c => c.PathKey, c => c.PathValue);
        }

        #endregion
    }
}
