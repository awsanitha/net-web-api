using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Base document filter that sorts Swagger paths by tag and method order attributes.
    /// </summary>
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region IDocumentFilter

        /// <inheritdoc />
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Helpers

        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item.Operations.TryGetValue(OperationType.Get, out var op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "GET";
            }
            if (item.Operations.TryGetValue(OperationType.Post, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "POST";
            }
            if (item.Operations.TryGetValue(OperationType.Put, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PUT";
            }
            if (item.Operations.TryGetValue(OperationType.Delete, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "DELETE";
            }
            if (item.Operations.TryGetValue(OperationType.Patch, out op))
            {
                tag = op.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PATCH";
            }

            return string.Empty;
        }

        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
                return;

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in swaggerDoc.Paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                var apiDesc = context.ApiDescriptions
                    .FirstOrDefault(a => ("/" + a.RelativePath?.TrimStart('/'))
                        .Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                if (!tagGroups.ContainsKey(tag))
                    tagGroups[tag] = new List<ApiOrder>();

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = GetApiOrder(apiDesc),
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                });

                tagGroups[tag] = tagGroups[tag].OrderBy(o => o.Order).ToList();
            }

            var list = tagGroups.SelectMany(g => g.Value).ToList();
            var sorted = list.OrderBy(a => a.OperationName)
                             .ToDictionary(a => a.PathKey, a => a.PathValue);

            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var kv in sorted)
                swaggerDoc.Paths.Add(kv.Key, kv.Value);
        }

        internal static int GetApiOrder(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
        {
            if (apiDescription == null) return -1;

            var endpoint = apiDescription.ActionDescriptor?.EndpointMetadata;
            if (endpoint == null) return -1;

            var attr = endpoint
                .OfType<SwaggerMethodOrderAttribute>()
                .FirstOrDefault();

            return attr?.Order ?? -1;
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
