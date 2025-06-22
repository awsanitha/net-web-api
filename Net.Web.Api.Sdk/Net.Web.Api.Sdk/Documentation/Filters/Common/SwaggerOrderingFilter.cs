using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;

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

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Get, out var getOperation))
            {
                tag = getOperation.Tags != null && getOperation.Tags.Count == 1 ? getOperation.Tags[0].Name : string.Empty;
                return "GET";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Put, out var putOperation))
            {
                tag = putOperation.Tags != null && putOperation.Tags.Count == 1 ? putOperation.Tags[0].Name : string.Empty;
                return "PUT";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Post, out var postOperation))
            {
                tag = postOperation.Tags != null && postOperation.Tags.Count == 1 ? postOperation.Tags[0].Name : string.Empty;
                return "POST";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Delete, out var deleteOperation))
            {
                tag = deleteOperation.Tags != null && deleteOperation.Tags.Count == 1 ? deleteOperation.Tags[0].Name : string.Empty;
                return "DELETE";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Options, out var optionsOperation))
            {
                tag = optionsOperation.Tags != null && optionsOperation.Tags.Count == 1 ? optionsOperation.Tags[0].Name : string.Empty;
                return "OPTIONS";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Head, out var headOperation))
            {
                tag = headOperation.Tags != null && headOperation.Tags.Count == 1 ? headOperation.Tags[0].Name : string.Empty;
                return "HEAD";
            }

            if (item.Operations.TryGetValue(Microsoft.OpenApi.Models.OperationType.Patch, out var patchOperation))
            {
                tag = patchOperation.Tags != null && patchOperation.Tags.Count == 1 ? patchOperation.Tags[0].Name : string.Empty;
                return "PATCH";
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
                var apiFound = context.ApiDescriptions.FirstOrDefault(c => c.RelativePath != null && $"{c.HttpMethod}{c.RelativePath}".StartsWith(apiKey));

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

            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var item in list.OrderBy(c => c.OperationName))
            {
                swaggerDoc.Paths.Add(item.PathKey, item.PathValue);
            }
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

            var methodInfo = apiDescription.ActionDescriptor.EndpointMetadata
                .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                .FirstOrDefault()?.MethodInfo;

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