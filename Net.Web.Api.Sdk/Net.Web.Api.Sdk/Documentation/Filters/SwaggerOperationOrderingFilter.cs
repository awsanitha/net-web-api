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

            var allOperationNames = GetOperationOrder(context);

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

            groups = groups.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

            var orderedPaths = new OpenApiPaths();

            foreach (var item in groups)
            {
                foreach (var api in item.Value)
                {
                    orderedPaths[api.Key] = api.Value;
                }
            }

            swaggerDoc.Paths = orderedPaths;
        }

        #endregion

        #region Private Methods

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[] GetOperationOrder(DocumentFilterContext context)
        {
            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var apiDesc in context.ApiDescriptions)
            {
                if (!(apiDesc.ActionDescriptor is ControllerActionDescriptor controllerAction))
                {
                    continue;
                }

                var controllerType = controllerAction.ControllerTypeInfo.AsType();

                var customAttribute = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                    as SwaggerOperationOrderAttribute ??
                    controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                    as SwaggerOperationOrderAttribute;

                if (customAttribute != null && !attributes.Any(a => a.From == customAttribute.From))
                {
                    attributes.Add(customAttribute);
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
                if (!result.Contains(cust))
                {
                    result.Add(cust);
                }
            }

            return result.ToArray();
        }

        #endregion
    }
}
