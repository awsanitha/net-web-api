using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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
        /// Applies the specified swagger document.
        /// </summary>
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Gets the invoke method and tag from a path item.
        /// </summary>
        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item.Operations.TryGetValue(OperationType.Get, out var op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "GET";
            }

            if (item.Operations.TryGetValue(OperationType.Put, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PUT";
            }

            if (item.Operations.TryGetValue(OperationType.Post, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "POST";
            }

            if (item.Operations.TryGetValue(OperationType.Delete, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "DELETE";
            }

            if (item.Operations.TryGetValue(OperationType.Options, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "OPTIONS";
            }

            if (item.Operations.TryGetValue(OperationType.Head, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "HEAD";
            }

            if (item.Operations.TryGetValue(OperationType.Patch, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PATCH";
            }

            return string.Empty;
        }

        /// <summary>
        /// Orderings the apply.
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
                    Order = GetApiOrder(path.Key, context),
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

            var ordered = list.OrderBy(c => c.OperationName).ToList();

            swaggerDoc.Paths.Clear();

            foreach (var item in ordered)
            {
                swaggerDoc.Paths.Add(item.PathKey, item.PathValue);
            }
        }

        /// <summary>
        /// Gets the API order for a path.
        /// </summary>
        internal static int GetApiOrder(string pathKey, DocumentFilterContext context)
        {
            var apiDescription = context.ApiDescriptions.FirstOrDefault(a => 
                ("/" + a.RelativePath?.TrimEnd('/')) == pathKey || 
                a.RelativePath == pathKey.TrimStart('/'));

            if (apiDescription == null)
            {
                return -1;
            }

            var actionDescriptor = apiDescription.ActionDescriptor;
            var methodInfo = (actionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.MethodInfo;

            if (methodInfo == null)
            {
                return -1;
            }

            var attr = methodInfo.GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false).FirstOrDefault();

            if (attr == null)
            {
                return -1;
            }

            return ((SwaggerMethodOrderAttribute)attr).Order;
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
