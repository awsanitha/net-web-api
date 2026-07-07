using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerOperationOrderingFilter. Orders Swagger operations by their configured operation order.
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        #region IDocumentFilter Implementations

        /// <inheritdoc />
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
                    groups[position] = new Dictionary<string, OpenApiPathItem> { { path.Key, path.Value } };
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

        #endregion

        #region Private Methods

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[]? GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return null;
            }

            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in paths)
            {
                var apiDescription = FindApiDescription(context.ApiDescriptions, path.Key);

                if (apiDescription?.ActionDescriptor == null)
                {
                    continue;
                }

                // Get controller type from action descriptor
                var controllerTypeMetadata = apiDescription.ActionDescriptor.EndpointMetadata
                    ?.OfType<ControllerTypeMetadata>()
                    .FirstOrDefault();

                System.Type? controllerType = null;

                // Try to get from the method's declaring type
                var methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                    ?.OfType<MethodInfo>()
                    .FirstOrDefault();

                if (methodInfo != null)
                {
                    controllerType = methodInfo.DeclaringType;
                }

                if (controllerType == null)
                {
                    continue;
                }

                var customAttribute =
                    controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                        .OfType<SwaggerOperationOrderAttribute>()
                        .FirstOrDefault()
                    ?? controllerType.BaseType?
                        .GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true)
                        .OfType<SwaggerOperationOrderAttribute>()
                        .FirstOrDefault();

                if (customAttribute == null)
                {
                    continue;
                }

                attributes.Add(customAttribute);
            }

            if (!attributes.Any())
            {
                return null;
            }

            var sdk = attributes.FirstOrDefault(c => c.From.Equals(SwaggerOperationOrderAttribute.OperationFrom.Sdk));
            var application = attributes.FirstOrDefault(c => c.From.Equals(SwaggerOperationOrderAttribute.OperationFrom.Application));

            if (application == null && sdk != null)
            {
                return sdk.OperationTags;
            }

            if (sdk == null && application != null)
            {
                return application.OperationTags;
            }

            if (application == null || sdk == null)
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

            return list.OrderBy(c => c.OperationName).ToDictionary(c => c.PathKey, c => c.PathValue);
        }

        private static Dictionary<int, IDictionary<string, OpenApiPathItem>> ProcessMethodOrdering(
            Dictionary<int, IDictionary<string, OpenApiPathItem>> groups,
            DocumentFilterContext context)
        {
            var result = new Dictionary<int, IDictionary<string, OpenApiPathItem>>();

            foreach (var item in groups)
            {
                result[item.Key] = ProcessItemMethodOrdering(item.Value, context);
            }

            return result;
        }

        #endregion

        #region Helper classes

        /// <summary>Marker metadata for controller type resolution.</summary>
        private class ControllerTypeMetadata
        {
            public System.Type ControllerType { get; set; } = typeof(object);
        }

        #endregion
    }
}
