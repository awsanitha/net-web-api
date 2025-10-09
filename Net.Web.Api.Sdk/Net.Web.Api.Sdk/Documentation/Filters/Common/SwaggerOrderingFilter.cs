using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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
        /// <param name="context">The context.</param>
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

            if (item.Operations.ContainsKey(OperationType.Get))
            {
                var operation = item.Operations[OperationType.Get];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "GET";
            }

            if (item.Operations.ContainsKey(OperationType.Put))
            {
                var operation = item.Operations[OperationType.Put];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "PUT";
            }

            if (item.Operations.ContainsKey(OperationType.Post))
            {
                var operation = item.Operations[OperationType.Post];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "POST";
            }

            if (item.Operations.ContainsKey(OperationType.Delete))
            {
                var operation = item.Operations[OperationType.Delete];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "DELETE";
            }

            if (item.Operations.ContainsKey(OperationType.Options))
            {
                var operation = item.Operations[OperationType.Options];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "OPTIONS";
            }

            if (item.Operations.ContainsKey(OperationType.Head))
            {
                var operation = item.Operations[OperationType.Head];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "HEAD";
            }

            if (item.Operations.ContainsKey(OperationType.Patch))
            {
                var operation = item.Operations[OperationType.Patch];
                tag = operation.Tags != null && operation.Tags.Count == 1 ? operation.Tags[0].Name : string.Empty;
                return "PATCH";
            }

            return string.Empty;
        }

        /// <summary>
        /// Orderings the apply.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
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
                var apiFound = context.ApiDescriptions.FirstOrDefault(c => c.RelativePath?.StartsWith(apiKey.TrimStart('/')) == true);

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

            swaggerDoc.Paths.Clear();
            foreach (var path in list.OrderBy(c => c.OperationName))
            {
                swaggerDoc.Paths.Add(path.PathKey, path.PathValue);
            }
        }

        /// <summary>
        /// Gets the API order.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetApiOrder(ApiDescription apiDescription)
        {
            if (apiDescription?.ActionDescriptor == null)
            {
                return -1;
            }

            var actionDescriptor = apiDescription.ActionDescriptor;
            var actionName = actionDescriptor.DisplayName;

            if (string.IsNullOrEmpty(actionName))
            {
                return -1;
            }

            // For ASP.NET Core, we need to get the method info differently
            var methodInfo = actionDescriptor.GetType().GetProperty("MethodInfo")?.GetValue(actionDescriptor) as System.Reflection.MethodInfo;

            if (methodInfo == null)
            {
                return -1;
            }

            var attr = methodInfo.GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false).FirstOrDefault();

            if (attr == null)
            {
                return -1;
            }

            return ((SwaggerMethodOrderAttribute)attr).Order;
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
