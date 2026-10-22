using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Base class for Swagger document ordering filters.
    /// </summary>
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region IDocumentFilter

        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item.Operations.TryGetValue(OperationType.Get, out var get))
            {
                tag = get.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "GET";
            }
            if (item.Operations.TryGetValue(OperationType.Put, out var put))
            {
                tag = put.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PUT";
            }
            if (item.Operations.TryGetValue(OperationType.Post, out var post))
            {
                tag = post.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "POST";
            }
            if (item.Operations.TryGetValue(OperationType.Delete, out var delete))
            {
                tag = delete.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "DELETE";
            }
            if (item.Operations.TryGetValue(OperationType.Options, out var options))
            {
                tag = options.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "OPTIONS";
            }
            if (item.Operations.TryGetValue(OperationType.Head, out var head))
            {
                tag = head.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "HEAD";
            }
            if (item.Operations.TryGetValue(OperationType.Patch, out var patch))
            {
                tag = patch.Tags?.FirstOrDefault()?.Name ?? string.Empty;
                return "PATCH";
            }

            return string.Empty;
        }

        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
                return;

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                var apiOrder = GetApiOrder(path.Key, path.Value, context);

                if (!tagGroups.ContainsKey(tag))
                    tagGroups.Add(tag, new List<ApiOrder>());

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = apiOrder,
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                });

                tagGroups[tag] = tagGroups[tag].OrderBy(c => c.Order).ToList();
            }

            var list = new List<ApiOrder>();

            foreach (var tagGroup in tagGroups)
                list.AddRange(tagGroup.Value);

            swaggerDoc.Paths = new OpenApiPaths();

            foreach (var item in list.OrderBy(c => c.OperationName))
                swaggerDoc.Paths.Add(item.PathKey, item.PathValue);
        }

        internal static int GetApiOrder(string pathKey, OpenApiPathItem pathItem, DocumentFilterContext context)
        {
            GetInvokeMethod(pathItem, out _);

            var httpMethod = pathItem.Operations.Keys.FirstOrDefault().ToString().ToUpper();
            var apiDescription = context.ApiDescriptions
                .FirstOrDefault(d =>
                    d.HttpMethod?.Equals(httpMethod, System.StringComparison.OrdinalIgnoreCase) == true &&
                    d.RelativePath != null &&
                    ("/" + d.RelativePath).Equals(pathKey, System.StringComparison.OrdinalIgnoreCase));

            if (apiDescription?.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
            {
                var attr = descriptor.MethodInfo.GetCustomAttribute<SwaggerMethodOrderAttribute>();
                return attr?.Order ?? -1;
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
