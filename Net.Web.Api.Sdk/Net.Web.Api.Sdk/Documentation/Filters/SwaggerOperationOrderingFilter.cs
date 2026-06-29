using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Orders Swagger operations by the tags defined in <see cref="SwaggerOperationOrderAttribute"/>.
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
            {
                OrderingApply(swaggerDoc, context);
                return;
            }

            var allTags = GetOperationOrder(swaggerDoc, context);

            if (allTags == null || allTags.Length == 0)
            {
                OrderingApply(swaggerDoc, context);
                return;
            }

            var groups = new Dictionary<int, IDictionary<string, OpenApiPathItem>>();

            foreach (var path in swaggerDoc.Paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                var position = allTags.ToList().IndexOf(tag);
                if (position == -1) position = int.MaxValue;

                if (!groups.ContainsKey(position))
                    groups[position] = new Dictionary<string, OpenApiPathItem>();

                groups[position][path.Key] = path.Value;
            }

            groups = ProcessMethodOrdering(groups, context);
            var result = groups.OrderBy(c => c.Key)
                               .SelectMany(g => g.Value)
                               .ToDictionary(c => c.Key, c => c.Value);

            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var item in result)
                swaggerDoc.Paths.Add(item.Key, item.Value);
        }

        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in swaggerDoc.Paths)
            {
                var apiDesc = context.ApiDescriptions
                    .FirstOrDefault(d => ("/" + d.RelativePath?.TrimStart('/'))
                        .Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                if (apiDesc?.ActionDescriptor is not ControllerActionDescriptor descriptor) continue;

                var controllerType = descriptor.ControllerTypeInfo.AsType();

                var attr = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                                         .FirstOrDefault() as SwaggerOperationOrderAttribute
                        ?? controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                                         .FirstOrDefault() as SwaggerOperationOrderAttribute;

                if (attr != null) attributes.Add(attr);
            }

            if (!attributes.Any()) return null;

            var sdk = attributes.FirstOrDefault(a => a.From == SwaggerOperationOrderAttribute.OperationFrom.Sdk);
            var app = attributes.FirstOrDefault(a => a.From == SwaggerOperationOrderAttribute.OperationFrom.Application);

            if (app == null) return sdk?.OperationTags;
            if (sdk == null) return app.OperationTags;

            var result = sdk.OperationTags.ToList();
            foreach (var tag in app.OperationTags)
                if (!result.Contains(tag)) result.Add(tag);

            return result.ToArray();
        }

        private static IDictionary<string, OpenApiPathItem> ProcessItemMethodOrdering(
            IDictionary<string, OpenApiPathItem> group, DocumentFilterContext context)
        {
            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in group)
            {
                GetInvokeMethod(path.Value, out var tag);
                if (!tagGroups.ContainsKey(tag)) tagGroups[tag] = new List<ApiOrder>();

                var apiDesc = context.ApiDescriptions
                    .FirstOrDefault(d => ("/" + d.RelativePath?.TrimStart('/'))
                        .Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = GetApiOrder(apiDesc),
                    PathKey = path.Key,
                    PathValue = path.Value,
                    OperationName = tag
                });
                tagGroups[tag] = tagGroups[tag].OrderBy(c => c.Order).ToList();
            }

            return tagGroups.SelectMany(g => g.Value)
                            .OrderBy(c => c.OperationName)
                            .ToDictionary(c => c.PathKey, c => c.PathValue);
        }

        private static Dictionary<int, IDictionary<string, OpenApiPathItem>> ProcessMethodOrdering(
            Dictionary<int, IDictionary<string, OpenApiPathItem>> groups, DocumentFilterContext context)
        {
            return groups.ToDictionary(g => g.Key,
                g => ProcessItemMethodOrdering(g.Value, context));
        }
    }
}
