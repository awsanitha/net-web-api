using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters.Common
{
    /// <summary>
    /// Class SwaggerOrderingFilter.
    /// Implements the <see cref="IDocumentFilter" />
    /// </summary>
    /// <seealso cref="IDocumentFilter" />
    public abstract class SwaggerOrderingFilter : IDocumentFilter
    {
        #region Public Virtual Methods

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        public virtual void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            OrderingApply(swaggerDoc, context);
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Gets the invoke method.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>System.String.</returns>
        internal static string GetInvokeMethod(OpenApiPathItem item, out string tag)
        {
            tag = string.Empty;

            if (item?.Operations == null || item.Operations.Count == 0)
            {
                return string.Empty;
            }

            var first = item.Operations.First();
            var operation = first.Value;

            tag = operation?.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;

            return first.Key.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Finds the API description matching a swagger path entry.
        /// </summary>
        /// <param name="context">The document filter context.</param>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="pathKey">The swagger path key.</param>
        /// <returns>ApiDescription.</returns>
        internal static ApiDescription FindApiDescription(DocumentFilterContext context, string verb, string pathKey)
        {
            var normalizedPath = (pathKey ?? string.Empty).TrimStart('/');

            return context?.ApiDescriptions?.FirstOrDefault(description =>
                string.Equals(description.HttpMethod, verb, StringComparison.OrdinalIgnoreCase) &&
                string.Equals((description.RelativePath ?? string.Empty).TrimStart('/'), normalizedPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Orderings the apply.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc?.Paths;

            if (paths == null || !paths.Any())
            {
                return;
            }

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in paths)
            {
                var key = GetInvokeMethod(path.Value, out var tag);
                var apiFound = FindApiDescription(context, key, path.Key);

                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups.Add(tag, new List<ApiOrder>());
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

            swaggerDoc.Paths = ToPaths(list.OrderBy(c => c.OperationName));
        }

        /// <summary>
        /// Gets the API order.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetApiOrder(ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription?.ActionDescriptor as ControllerActionDescriptor;
            var actionMethod = actionDescriptor?.MethodInfo;

            if (actionMethod == null)
            {
                return -1;
            }

            var attr = actionMethod.GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false).FirstOrDefault();

            if (attr == null)
            {
                return -1;
            }

            return ((SwaggerMethodOrderAttribute)attr).Order;
        }

        /// <summary>
        /// Builds an <see cref="OpenApiPaths" /> instance preserving the supplied order.
        /// </summary>
        /// <param name="items">The ordered items.</param>
        /// <returns>OpenApiPaths.</returns>
        internal static OpenApiPaths ToPaths(IEnumerable<ApiOrder> items)
        {
            var result = new OpenApiPaths();

            foreach (var item in items)
            {
                result[item.PathKey] = item.PathValue;
            }

            return result;
        }

        /// <summary>
        /// Gets the controller type of an API description.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        /// <returns>Type.</returns>
        internal static Type GetControllerType(ApiDescription apiDescription)
        {
            var actionDescriptor = apiDescription?.ActionDescriptor as ControllerActionDescriptor;

            return actionDescriptor?.ControllerTypeInfo?.AsType();
        }

        /// <summary>
        /// Gets a custom attribute declared on a method or on its controller type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="member">The member.</param>
        /// <returns>The attribute, or <c>null</c>.</returns>
        internal static T GetAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member?.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }

        #endregion

        #region Internal Class

        /// <summary>
        /// Class ApiOrder.
        /// </summary>
        internal class ApiOrder
        {
            #region Internal Properties

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>The order.</value>
            internal int Order { get; set; }

            /// <summary>
            /// Gets or sets the path key.
            /// </summary>
            /// <value>The path key.</value>
            internal string PathKey { get; set; }

            /// <summary>
            /// Gets or sets the path value.
            /// </summary>
            /// <value>The path value.</value>
            internal OpenApiPathItem PathValue { get; set; }

            /// <summary>
            /// Gets or sets the name of the operation.
            /// </summary>
            /// <value>The name of the operation.</value>
            internal string OperationName { get; set; }

            #endregion
        }

        #endregion
    }
}
