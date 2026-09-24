using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerOperationOrderingFilter.
    /// Implements the <see cref="SwaggerOrderingFilter" />
    /// </summary>
    /// <seealso cref="SwaggerOrderingFilter" />
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        #region IDocumentFilter Implementations

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
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

                IDictionary<string, OpenApiPathItem> dictionary;

                if (groups.ContainsKey(position))
                {
                    dictionary = groups[position];

                    dictionary.Add(path.Key, path.Value);
                }
                else
                {
                    dictionary = new Dictionary<string, OpenApiPathItem> { { path.Key, path.Value } };

                    groups.Add(position, dictionary);
                }
            }

            groups = ProcessMethodOrdering(groups, context);
            groups = groups.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

            var result = new OpenApiPaths();

            foreach (var item in groups)
            {
                foreach (var api in item.Value)
                {
                    result.Add(api.Key, api.Value);
                }
            }

            swaggerDoc.Paths = result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the operation order.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        /// <returns>System.String[].</returns>
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return null;
            }

            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var apiDesc in context.ApiDescriptions)
            {
                if (apiDesc.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad)
                {
                    var controllerType = cad.ControllerTypeInfo;

                    var customAttribute = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                        as SwaggerOperationOrderAttribute ??
                            controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                        as SwaggerOperationOrderAttribute;

                    if (customAttribute != null && !attributes.Contains(customAttribute))
                    {
                        attributes.Add(customAttribute);
                    }
                }
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

            if (application == null)
            {
                return null;
            }

            var result = sdk.OperationTags.ToList();

            foreach (var cust in application.OperationTags)
            {
                if (result.Contains(cust))
                {
                    continue;
                }

                result.Add(cust);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Processes the item method ordering.
        /// </summary>
        private static IDictionary<string, OpenApiPathItem> ProcessItemMethodOrdering(IDictionary<string, OpenApiPathItem> group,
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

                var item = new ApiOrder
                {
                    Order = GetApiOrder(context, path.Key),
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

        /// <summary>
        /// Processes the method ordering.
        /// </summary>
        private static Dictionary<int, IDictionary<string, OpenApiPathItem>> ProcessMethodOrdering(Dictionary<int, IDictionary<string, OpenApiPathItem>> groups,
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
