using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Configures a Swagger operation for multipart/form-data file upload based on <see cref="SwaggerUploadOperationAttribute"/>.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttribute<SwaggerUploadOperationAttribute>();

            if (upload == null)
            {
                return;
            }

            // Build the multipart/form-data request body schema from the parameter type's properties
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>()
            };

            var required = new System.Collections.Generic.HashSet<string>();

            foreach (var property in upload.ParameterType.GetProperties())
            {
                var isFile = typeof(IFormFile).IsAssignableFrom(property.PropertyType);

                schema.Properties[property.Name] = isFile
                    ? new OpenApiSchema { Type = "string", Format = "binary" }
                    : new OpenApiSchema { Type = "string" };

                var requiredAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>();
                if (requiredAttr != null)
                {
                    required.Add(property.Name);
                }
            }

            schema.Required = required;

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType { Schema = schema }
                }
            };
        }
    }
}
