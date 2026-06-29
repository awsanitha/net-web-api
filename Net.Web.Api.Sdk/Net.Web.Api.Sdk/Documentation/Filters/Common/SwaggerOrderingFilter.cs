using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Base class for Swagger document ordering filters.
    /// </summary>
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any()) return;

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in swaggerDoc.Paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                if (!tagGroups.ContainsKey(tag)) tagGroups[tag] = new List<ApiOrder>();

                var apiDesc = context.ApiDescriptions
                    .FirstOrDefault(d => NormalizePath(d.RelativePath) == NormalizePath(path.Key));

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = GetApiOrder(apiDesc),
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                });
                tagGroups[tag] = tagGroups[tag].OrderBy(c => c.Order).ToList();
            }

            var list = tagGroups.SelectMany(g => g.Value).ToList();
            var ordered = list.OrderBy(c => c.OperationName)
                              .ToDictionary(c => c.PathKey, c => c.PathValue);

            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var item in ordered)
                swaggerDoc.Paths.Add(item.Key, item.Value);
        }

        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;
            foreach (var op in item.Operations)
            {
                tag = op.Value.Tags?.Count == 1 ? op.Value.Tags[0].Name : string.Empty;
                return op.Key.ToString().ToUpper();
            }
            return string.Empty;
        }

        internal static int GetApiOrder(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDesc)
        {
            if (apiDesc == null) return -1;
            var descriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;
            var method = descriptor?.ControllerTypeInfo.GetMethod(descriptor.ActionName);
            var attr = method?.GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false).FirstOrDefault()
                as SwaggerMethodOrderAttribute;
            return attr?.Order ?? -1;
        }

        private static string NormalizePath(string path)
            => path?.TrimStart('/').ToLowerInvariant() ?? string.Empty;

        internal class ApiOrder
        {
            internal int Order { get; set; }
            internal string PathKey { get; set; }
            internal OpenApiPathItem PathValue { get; set; }
            internal string OperationName { get; set; }
        }
    }
}
