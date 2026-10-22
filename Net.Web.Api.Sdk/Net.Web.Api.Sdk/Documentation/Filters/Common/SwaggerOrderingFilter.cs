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
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region Public Virtual Methods

        /// <summary>
        /// Applies the ordering to the Swagger document.
        /// </summary>
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Gets the HTTP method and primary tag from a path item.
        /// </summary>
        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            foreach (var operation in item.Operations)
            {
                tag = operation.Value.Tags?.Count == 1
                    ? operation.Value.Tags[0].Name
                    : string.Empty;

                return operation.Key.ToString().ToUpper();
            }

            return string.Empty;
        }

        /// <summary>
        /// Default ordering: sort by tag name and operation order.
        /// </summary>
        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
            {
                return;
            }

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in swaggerDoc.Paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                var apiFound = FindApiDescription(context, path.Key, path.Value);

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

            var sorted = list.OrderBy(c => c.OperationName)
                             .ToDictionary(c => c.PathKey, c => c.PathValue);

            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var entry in sorted)
            {
                swaggerDoc.Paths.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Gets the swagger method order from the action method's <see cref="SwaggerMethodOrderAttribute"/>.
        /// </summary>
        internal static int GetApiOrder(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
        {
            if (apiDescription == null)
            {
                return -1;
            }

            var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
            var methodInfo = actionDescriptor?.MethodInfo;

            if (methodInfo == null)
            {
                return -1;
            }

            var attr = methodInfo.GetCustomAttribute<SwaggerMethodOrderAttribute>(false);

            return attr?.Order ?? -1;
        }

        /// <summary>
        /// Finds the <see cref="Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription"/> matching a path/item.
        /// </summary>
        internal static Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription FindApiDescription(
            DocumentFilterContext context,
            string pathKey,
            OpenApiPathItem pathItem)
        {
            var httpMethod = GetInvokeMethod(pathItem, out _);
            var trimmedPath = pathKey.TrimStart('/');

            return context.ApiDescriptions.FirstOrDefault(a =>
                a.HttpMethod?.Equals(httpMethod, System.StringComparison.OrdinalIgnoreCase) == true &&
                (a.RelativePath?.TrimStart('/').Equals(trimmedPath, System.StringComparison.OrdinalIgnoreCase) == true ||
                 trimmedPath.StartsWith(a.RelativePath?.TrimStart('/') ?? string.Empty, System.StringComparison.OrdinalIgnoreCase)));
        }

        #endregion

        #region Internal Class

        /// <summary>
        /// Represents an API operation with its ordering information.
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
