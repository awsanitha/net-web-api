using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerOperationOrderingFilter.
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
                return;

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
                var position = allOperationNames.ToList().IndexOf(tag);

                if (position == -1)
                    position = int.MaxValue;

                if (groups.ContainsKey(position))
                    groups[position].Add(path.Key, path.Value);
                else
                    groups.Add(position, new Dictionary<string, OpenApiPathItem> { { path.Key, path.Value } });
            }

            groups = ProcessMethodOrdering(groups, context);
            groups = groups.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

            swaggerDoc.Paths = new OpenApiPaths();

            foreach (var item in groups)
                foreach (var api in item.Value)
                    swaggerDoc.Paths.Add(api.Key, api.Value);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
                return null;

            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in paths)
            {
                GetInvokeMethod(path.Value, out _);
                var httpMethod = path.Value.Operations.Keys.FirstOrDefault().ToString().ToUpper();

                var apiDescription = context.ApiDescriptions
                    .FirstOrDefault(d =>
                        d.HttpMethod?.Equals(httpMethod, System.StringComparison.OrdinalIgnoreCase) == true &&
                        d.RelativePath != null &&
                        ("/" + d.RelativePath).Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                if (apiDescription?.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
                    continue;

                var controllerType = descriptor.ControllerTypeInfo.AsType();

                var customAttribute = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                    .FirstOrDefault() as SwaggerOperationOrderAttribute
                    ?? controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                    .FirstOrDefault() as SwaggerOperationOrderAttribute;

                if (customAttribute != null)
                    attributes.Add(customAttribute);
            }

            if (!attributes.Any())
                return null;

            var sdk = attributes.FirstOrDefault(c => c.From.Equals(SwaggerOperationOrderAttribute.OperationFrom.Sdk));
            var application = attributes.FirstOrDefault(c => c.From.Equals(SwaggerOperationOrderAttribute.OperationFrom.Application));

            if (application == null && sdk != null)
                return sdk.OperationTags;

            if (sdk == null && application != null)
                return application.OperationTags;

            if (application == null)
                return null;

            var result = sdk.OperationTags.ToList();

            foreach (var cust in application.OperationTags)
            {
                if (!result.Contains(cust))
                    result.Add(cust);
            }

            return result.ToArray();
        }

        private static IDictionary<string, OpenApiPathItem> ProcessItemMethodOrdering(IDictionary<string, OpenApiPathItem> group, DocumentFilterContext context)
        {
            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in group)
            {
                GetInvokeMethod(path.Value, out var tag);
                var order = GetApiOrder(path.Key, path.Value, context);

                if (!tagGroups.ContainsKey(tag))
                    tagGroups.Add(tag, new List<ApiOrder>());

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
                list.AddRange(tagGroup.Value);

            return list.OrderBy(c => c.OperationName).ToDictionary(c => c.PathKey, c => c.PathValue);
        }

        private static Dictionary<int, IDictionary<string, OpenApiPathItem>> ProcessMethodOrdering(
            Dictionary<int, IDictionary<string, OpenApiPathItem>> groups,
            DocumentFilterContext context)
        {
            return groups.ToDictionary(item => item.Key, item => ProcessItemMethodOrdering(item.Value, context));
        }
    }
}
