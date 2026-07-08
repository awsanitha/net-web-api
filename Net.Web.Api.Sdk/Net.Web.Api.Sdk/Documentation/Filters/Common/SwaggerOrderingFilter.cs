using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Class SwaggerOrderingFilter. Abstract base for Swagger document ordering filters.
    /// </summary>
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region Public Virtual Methods

        /// <inheritdoc />
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

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
            if (item.Operations.TryGetValue(OperationType.Options, out var optOp))
            {
                tag = optOp.Tags?.FirstOrDefault()?.Name ?? string.Empty;
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
                var apiFound = FindApiDescription(context.ApiDescriptions, path.Key);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups[tag] = new List<ApiOrder>();
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

            var ordered = list.OrderBy(c => c.OperationName).ToDictionary(c => c.PathKey, c => c.PathValue);
            swaggerDoc.Paths.Clear();

            foreach (var item in ordered)
            {
                swaggerDoc.Paths.Add(item.Key, item.Value);
            }
        }

        internal static ApiDescription? FindApiDescription(IEnumerable<ApiDescription> apiDescriptions, string pathKey)
        {
            var normalizedPath = pathKey.TrimStart('/');
            return apiDescriptions.FirstOrDefault(a =>
                a.RelativePath != null &&
                a.RelativePath.Equals(normalizedPath, System.StringComparison.OrdinalIgnoreCase));
        }

        internal static int GetApiOrder(ApiDescription? apiDescription)
        {
            if (apiDescription?.ActionDescriptor == null)
            {
                return -1;
            }

            // Resolve the action method from ActionDescriptor metadata
            var methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                ?.OfType<MethodInfo>()
                .FirstOrDefault();

            if (methodInfo == null)
            {
                return -1;
            }

            var attr = methodInfo.GetCustomAttribute<SwaggerMethodOrderAttribute>();
            return attr?.Order ?? -1;
        }

        #endregion

        #region Internal Class

        internal class ApiOrder
        {
            internal int Order { get; set; }
            internal string PathKey { get; set; } = string.Empty;
            internal OpenApiPathItem PathValue { get; set; } = new OpenApiPathItem();
            internal string OperationName { get; set; } = string.Empty;
        }

        #endregion
    }
}
