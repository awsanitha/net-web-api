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
    /// Orders API operations based on SwaggerOperationOrderAttribute on controllers.
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return;
            }

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
                {
                    position = int.MaxValue;
                }

                if (groups.ContainsKey(position))
                {
                    groups[position].Add(path.Key, path.Value);
                }
                else
                {
                    groups.Add(position, new Dictionary<string, OpenApiPathItem> { { path.Key, path.Value } });
                }
            }

            groups = ProcessMethodOrdering(groups, context);
            groups = groups.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

            swaggerDoc.Paths.Clear();

            foreach (var item in groups)
            {
                foreach (var api in item.Value)
                {
                    swaggerDoc.Paths.Add(api.Key, api.Value);
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var apiDescription in context.ApiDescriptions)
            {
                var controllerType = (apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
                    ?.ControllerTypeInfo?.AsType();

                if (controllerType == null) continue;

                var customAttribute = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                    .Cast<SwaggerOperationOrderAttribute>()
                    .FirstOrDefault()
                    ?? controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                    .Cast<SwaggerOperationOrderAttribute>()
                    .FirstOrDefault();

                if (customAttribute != null && !attributes.Any(a => a.From == customAttribute.From))
                {
                    attributes.Add(customAttribute);
                }
            }

            if (!attributes.Any())
            {
                return null;
            }

            var sdk = attributes.FirstOrDefault(c => c.From == SwaggerOperationOrderAttribute.OperationFrom.Sdk);
            var application = attributes.FirstOrDefault(c => c.From == SwaggerOperationOrderAttribute.OperationFrom.Application);

            if (application == null && sdk != null)
            {
                return sdk.OperationTags;
            }

            if (sdk == null && application != null)
            {
                return application.OperationTags;
            }

            if (application == null)
            {
                return null;
            }

            var result = sdk.OperationTags.ToList();

            foreach (var cust in application.OperationTags)
            {
                if (!result.Contains(cust))
                {
                    result.Add(cust);
                }
            }

            return result.ToArray();
        }

        private static IDictionary<string, OpenApiPathItem> ProcessItemMethodOrdering(
            IDictionary<string, OpenApiPathItem> group,
            DocumentFilterContext context)
        {
            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in group)
            {
                GetInvokeMethod(path.Value, out var tag);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
                }

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = GetApiOrderFromContext(path.Value, context),
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

            return list.OrderBy(c => c.OperationName).ToDictionary(c => c.PathKey, c => c.PathValue);
        }

        private static int GetApiOrderFromContext(OpenApiPathItem pathItem, DocumentFilterContext context)
        {
            // Find the method info for this path's operation in context
            var invokeMethod = string.Empty;
            OperationType opType = OperationType.Get;

            foreach (var op in pathItem.Operations)
            {
                opType = op.Key;
                break;
            }

            foreach (var apiDesc in context.ApiDescriptions)
            {
                var method = apiDesc.HttpMethod?.ToUpper();
                if (method == opType.ToString().ToUpper())
                {
                    var actionDescriptor = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                    if (actionDescriptor != null)
                    {
                        var attr = actionDescriptor.MethodInfo
                            .GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false)
                            .Cast<SwaggerMethodOrderAttribute>()
                            .FirstOrDefault();

                        if (attr != null) return attr.Order;
                    }
                }
            }

            return -1;
        }

        private static Dictionary<int, IDictionary<string, OpenApiPathItem>> ProcessMethodOrdering(
            Dictionary<int, IDictionary<string, OpenApiPathItem>> groups,
            DocumentFilterContext context)
        {
            var result = new Dictionary<int, IDictionary<string, OpenApiPathItem>>();

            foreach (var item in groups)
            {
                result.Add(item.Key, ProcessItemMethodOrdering(item.Value, context));
            }

            return result;
        }
    }
}
