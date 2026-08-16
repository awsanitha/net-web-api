using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Operation filter that converts a multipart upload action into the proper Swagger form-data
    /// representation, driven by <see cref="SwaggerUploadOperationAttribute"/>.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo
                ?.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), true)
                .OfType<SwaggerUploadOperationAttribute>()
                .FirstOrDefault();

            if (upload == null) return;

            // Build a multipart/form-data request body from the model type's properties.
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>()
            };

            foreach (var prop in upload.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propSchema = IsFileProperty(prop)
                    ? new OpenApiSchema { Type = "string", Format = "binary" }
                    : new OpenApiSchema { Type = "string" };

                schema.Properties[prop.Name] = propSchema;
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType { Schema = schema }
                }
            };

            // Clear any auto-generated parameters so they don't conflict
            operation.Parameters.Clear();
        }

        private static bool IsFileProperty(PropertyInfo prop)
        {
            return typeof(Microsoft.AspNetCore.Http.IFormFile).IsAssignableFrom(prop.PropertyType);
        }
    }
}
