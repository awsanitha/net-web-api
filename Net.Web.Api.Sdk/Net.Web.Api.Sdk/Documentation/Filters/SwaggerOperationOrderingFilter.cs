using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Document filter that sorts Swagger operations by <see cref="SwaggerOperationOrderAttribute"/> tags.
    /// </summary>
    public class SwaggerOperationOrderingFilter : SwaggerOrderingFilter
    {
        /// <inheritdoc />
        public override void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Paths == null || !swaggerDoc.Paths.Any())
                return;

            var allOperationTags = GetOperationOrder(swaggerDoc, context);

            if (allOperationTags == null || allOperationTags.Length == 0)
            {
                OrderingApply(swaggerDoc, context);
                return;
            }

            var groups = new Dictionary<int, IDictionary<string, OpenApiPathItem>>();

            foreach (var path in swaggerDoc.Paths)
            {
                GetInvokeMethod(path.Value, out var tag);
                var position = System.Array.IndexOf(allOperationTags, tag);
                if (position == -1) position = int.MaxValue;

                if (!groups.ContainsKey(position))
                    groups[position] = new Dictionary<string, OpenApiPathItem>();

                groups[position][path.Key] = path.Value;
            }

            groups = groups.OrderBy(g => g.Key)
                           .ToDictionary(g => g.Key, g => g.Value);

            var result = new OpenApiPaths();
            foreach (var group in groups)
                foreach (var path in group.Value)
                    result.Add(path.Key, path.Value);

            swaggerDoc.Paths = result;
        }

        private static string[] GetOperationOrder(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var attributes = new List<SwaggerOperationOrderAttribute>();

            foreach (var path in swaggerDoc.Paths)
            {
                var apiDesc = context.ApiDescriptions
                    .FirstOrDefault(a => ("/" + a.RelativePath?.TrimStart('/'))
                        .Equals(path.Key, System.StringComparison.OrdinalIgnoreCase));

                if (apiDesc == null) continue;

                var metadata = apiDesc.ActionDescriptor?.EndpointMetadata;
                if (metadata == null) continue;

                var attr = metadata.OfType<SwaggerOperationOrderAttribute>().FirstOrDefault();
                if (attr != null) attributes.Add(attr);
            }

            if (!attributes.Any()) return null;

            var sdk = attributes.FirstOrDefault(a => a.From == SwaggerOperationOrderAttribute.OperationFrom.Sdk);
            var app = attributes.FirstOrDefault(a => a.From == SwaggerOperationOrderAttribute.OperationFrom.Application);

            if (app == null && sdk != null) return sdk.OperationTags;
            if (sdk == null && app != null) return app.OperationTags;
            if (app == null) return null;

            var result = sdk.OperationTags.ToList();
            foreach (var tag in app.OperationTags)
            {
                if (!result.Contains(tag)) result.Add(tag);
            }
            return result.ToArray();
        }
    }
}
