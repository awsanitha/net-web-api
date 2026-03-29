using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return null;
            }

            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in paths)
            {
                var apiDescription = context.ApiDescriptions.FirstOrDefault(a =>
                    ("/" + a.RelativePath?.TrimStart('/')).Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                if (apiDescription == null)
                {
                    continue;
                }

                var actionDescriptor = apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                var controllerType = actionDescriptor?.ControllerTypeInfo?.AsType();

                if (controllerType == null)
                {
                    continue;
                }

                var customAttribute = controllerType.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                    as SwaggerOperationOrderAttribute ??
                    controllerType.BaseType?.GetCustomAttributes(typeof(SwaggerOperationOrderAttribute), true).FirstOrDefault()
                    as SwaggerOperationOrderAttribute;

                if (customAttribute != null)
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
