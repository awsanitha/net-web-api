using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            var operationMethodPairs = new[]
            {
                (OperationType.Get, "GET"),
                (OperationType.Put, "PUT"),
                (OperationType.Post, "POST"),
                (OperationType.Delete, "DELETE"),
                (OperationType.Options, "OPTIONS"),
                (OperationType.Head, "HEAD"),
                (OperationType.Patch, "PATCH")
            };

            foreach (var (opType, methodName) in operationMethodPairs)
            {
                if (item.Operations.TryGetValue(opType, out var operation))
                {
                    tag = operation.Tags != null && operation.Tags.Count == 1
                        ? operation.Tags[0].Name
                        : string.Empty;

                    return methodName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Orderings the apply.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        internal static void OrderingApply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;

            if (paths == null || !paths.Any())
            {
                return;
            }

            var tagGroups = new Dictionary<string, IList<ApiOrder>>();

            foreach (var path in paths)
            {
                var key = GetInvokeMethod(path.Value, out var tag);
                var apiKey = $"{key}{path.Key.TrimStart('/')}";
                var apiFound = context.ApiDescriptions.FirstOrDefault(c => c.RelativePath != null &&
                    apiKey.Contains(c.RelativePath.TrimStart('/')));

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

            var orderedPaths = new OpenApiPaths();

            foreach (var apiOrder in list.OrderBy(c => c.OperationName))
            {
                orderedPaths.Add(apiOrder.PathKey, apiOrder.PathValue);
            }

            swaggerDoc.Paths = orderedPaths;
        }

        /// <summary>
        /// Gets the API order.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetApiOrder(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
        {
            if (apiDescription?.ActionDescriptor == null)
            {
                return -1;
            }

            var actionDescriptor = apiDescription.ActionDescriptor;

            if (actionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
            {
                var actionMethod = controllerActionDescriptor.MethodInfo;

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

            return -1;
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
