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

            foreach (var op in item.Operations)
            {
                if (op.Value.Tags != null && op.Value.Tags.Count == 1)
                {
                    tag = op.Value.Tags[0].Name;
                }

                switch (op.Key)
                {
                    case OperationType.Get: return "GET";
                    case OperationType.Put: return "PUT";
                    case OperationType.Post: return "POST";
                    case OperationType.Delete: return "DELETE";
                    case OperationType.Options: return "OPTIONS";
                    case OperationType.Head: return "HEAD";
                    case OperationType.Patch: return "PATCH";
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

            var orderedPaths = new OpenApiPaths();
            foreach (var item in list.OrderBy(c => c.OperationName))
            {
                orderedPaths.Add(item.PathKey, item.PathValue);
            }

            swaggerDoc.Paths = orderedPaths;
        }

        /// <summary>
        /// Gets the API order.
        /// </summary>
        /// <param name="context">The document filter context.</param>
        /// <param name="pathKey">The path key.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetApiOrder(DocumentFilterContext context, string pathKey)
        {
            var apiDescription = context.ApiDescriptions
                .FirstOrDefault(a => "/" + a.RelativePath == pathKey || a.RelativePath == pathKey.TrimStart('/'));

            if (apiDescription == null)
            {
                return -1;
            }

            var controllerType = apiDescription.ActionDescriptor?.EndpointMetadata
                ?.OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                .FirstOrDefault()?.ControllerTypeInfo;

            if (controllerType == null)
            {
                // Try from the ActionDescriptor directly
                if (apiDescription.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad)
                {
                    controllerType = cad.ControllerTypeInfo;
                    var methodInfo = cad.MethodInfo;

                    var attr = methodInfo?.GetCustomAttributes(typeof(SwaggerMethodOrderAttribute), false).FirstOrDefault();

                    if (attr != null)
                    {
                        return ((SwaggerMethodOrderAttribute)attr).Order;
                    }
                }

                return -1;
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
