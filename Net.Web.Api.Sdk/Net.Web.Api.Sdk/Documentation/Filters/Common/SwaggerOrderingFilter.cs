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
    /// Base class for document-level Swagger filters that order operations.
    /// </summary>
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region Public Virtual Methods

        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item.Operations.ContainsKey(OperationType.Get))
            {
                tag = item.Operations[OperationType.Get].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "GET";
            }

            if (item.Operations.ContainsKey(OperationType.Put))
            {
                tag = item.Operations[OperationType.Put].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PUT";
            }

            if (item.Operations.ContainsKey(OperationType.Post))
            {
                tag = item.Operations[OperationType.Post].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "POST";
            }

            if (item.Operations.ContainsKey(OperationType.Delete))
            {
                tag = item.Operations[OperationType.Delete].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "DELETE";
            }

            if (item.Operations.ContainsKey(OperationType.Options))
            {
                tag = item.Operations[OperationType.Options].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "OPTIONS";
            }

            if (item.Operations.ContainsKey(OperationType.Head))
            {
                tag = item.Operations[OperationType.Head].Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "HEAD";
            }

            if (item.Operations.ContainsKey(OperationType.Patch))
            {
                tag = item.Operations[OperationType.Patch].Tags?.FirstOrDefault()?.Name ?? string.Empty;
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
                var order = GetApiOrder(path.Value);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
                }

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = order,
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                });

                tagGroups[tag] = tagGroups[tag].OrderBy(c => c.Order).ToList();
            }

            var list = new List<ApiOrder>();

            foreach (var tagGroup in tagGroups)
            {
                list.AddRange(tagGroup.Value);
            }

            var ordered = list.OrderBy(c => c.OperationName)
                              .ToDictionary(c => c.PathKey, c => c.PathValue);

            swaggerDoc.Paths.Clear();

            foreach (var item in ordered)
            {
                swaggerDoc.Paths.Add(item.Key, item.Value);
            }
        }

        internal static int GetApiOrder(OpenApiPathItem pathItem)
        {
            foreach (var op in pathItem.Operations.Values)
            {
                if (op.Extensions.TryGetValue("x-order", out var orderExt))
                {
                    return 0; // Default if found
                }
            }

            return -1;
        }

        #endregion

        #region Internal Class

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
