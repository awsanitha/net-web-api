using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Replaces parameters with file upload fields when <see cref="SwaggerUploadOperationAttribute"/> is present.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), false)
                              .FirstOrDefault() as SwaggerUploadOperationAttribute;

            if (attr == null) return;

            operation.Parameters.Clear();

            var schema = context.SchemaRepository.Schemas.TryGetValue(attr.ParameterType.Name, out var s) ? s : null;
            if (schema?.Properties == null) return;

            var content = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = schema.Properties.ToDictionary(
                        p => p.Key,
                        p => string.IsNullOrEmpty(p.Value.Reference?.Id) && p.Value.Type != "string"
                            ? p.Value
                            : new OpenApiSchema { Type = "string", Format = "binary" })
                }
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = { ["multipart/form-data"] = content }
            };
        }
    }
}
