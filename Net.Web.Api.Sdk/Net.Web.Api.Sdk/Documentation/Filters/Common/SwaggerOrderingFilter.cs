using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Class SwaggerOrderingFilter.
    /// Implements the <see cref="IDocumentFilter" />
    /// </summary>
    /// <seealso cref="IDocumentFilter" />
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region Public Virtual Methods

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Gets the HTTP method and tag from a path item.
        /// </summary>
        /// <param name="item">The path item.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>System.String (HTTP method).</returns>
        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item.Operations.TryGetValue(OperationType.Get, out var getOp))
            {
                tag = getOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "GET";
            }

            if (item.Operations.TryGetValue(OperationType.Put, out var putOp))
            {
                tag = putOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PUT";
            }

            if (item.Operations.TryGetValue(OperationType.Post, out var postOp))
            {
                tag = postOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "POST";
            }

            if (item.Operations.TryGetValue(OperationType.Delete, out var deleteOp))
            {
                tag = deleteOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "DELETE";
            }

            if (item.Operations.TryGetValue(OperationType.Options, out var optionsOp))
            {
                tag = optionsOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "OPTIONS";
            }

            if (item.Operations.TryGetValue(OperationType.Head, out var headOp))
            {
                tag = headOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "HEAD";
            }

            if (item.Operations.TryGetValue(OperationType.Patch, out var patchOp))
            {
                tag = patchOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PATCH";
            }

            return string.Empty;
        }

        /// <summary>
        /// Ordering apply implementation.
        /// </summary>
        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return;
            }

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in paths)
            {
                GetInvokeMethod(path.Value, out var tag);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
                }

                var item = new ApiOrder
                {
                    Order = GetApiOrder(path.Key, path.Value, context),
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

            var orderedPaths = new OpenApiPaths();
            foreach (var item in list.OrderBy(c => c.OperationName))
            {
                orderedPaths[item.PathKey] = item.PathValue;
            }
            swaggerDoc.Paths = orderedPaths;
        }

        /// <summary>
        /// Gets the API order for a path.
        /// </summary>
        internal static int GetApiOrder(string pathKey, OpenApiPathItem pathItem, DocumentFilterContext context)
        {
            foreach (var apiDesc in context.ApiDescriptions)
            {
                if (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerAction)
                {
                    var actionMethod = controllerAction.MethodInfo;
                    var attr = actionMethod?.GetCustomAttribute<SwaggerMethodOrderAttribute>(false);

                    if (attr != null)
                    {
                        // Match by checking if the path matches the route template
                        var template = apiDesc.RelativePath;
                        if (pathKey.TrimStart('/').Equals(template?.TrimStart('/'), System.StringComparison.OrdinalIgnoreCase))
                        {
                            return attr.Order;
                        }
                    }
                }
            }

            return -1;
        }

        #endregion

        #region Internal Class

        /// <summary>
        /// Class ApiOrder.
        /// </summary>
        internal class ApiOrder
        {
            internal int Order { get; set; }
            internal string PathKey { get; set; }
            internal OpenApiPathItem PathValue { get; set; }
            internal string OperationName { get; set; }
        }

        #endregion
    }
}
