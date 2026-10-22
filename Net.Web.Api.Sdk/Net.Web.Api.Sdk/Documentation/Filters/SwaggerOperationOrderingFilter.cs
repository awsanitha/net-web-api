using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerOperationOrderingFilter.
    /// Implements the <see cref="SwaggerOrderingFilter" />
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        #region IDocumentFilter Implementations

        /// <inheritdoc />
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
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

            foreach (var path in swaggerDoc.Paths)
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

            swaggerDoc.Paths = new OpenApiPaths();

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
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
            {
                return null;
            }

            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in swaggerDoc.Paths)
            {
                var apiFound = FindApiDescription(context, path.Key, path.Value);

                if (apiFound == null)
                {
                    continue;
                }

                var actionDescriptor = apiFound.ActionDescriptor as ControllerActionDescriptor;
                if (actionDescriptor == null)
                {
                    continue;
                }

                var controllerType = actionDescriptor.ControllerTypeInfo.AsType();

                var customAttribute =
                    controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault() as SwaggerOperationOrderAttribute
                    ?? controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault() as SwaggerOperationOrderAttribute;

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

            foreach (var tag in application.OperationTags)
            {
                if (!result.Contains(tag))
                {
                    result.Add(tag);
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
                var apiFound = FindApiDescription(context, path.Key, path.Value);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
                }

                tagGroups[tag].Add(new ApiOrder
                {
                    Order = GetApiOrder(apiFound),
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

            return list.OrderBy(c => c.OperationName)
                       .ToDictionary(c => c.PathKey, c => c.PathValue);
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

        #endregion
    }
}
