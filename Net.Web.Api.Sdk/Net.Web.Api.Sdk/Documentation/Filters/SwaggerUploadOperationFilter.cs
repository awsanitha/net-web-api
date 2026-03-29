using System.Linq;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), true)
                .FirstOrDefault() as SwaggerUploadOperationAttribute;

            if (upload == null)
            {
                return;
            }

            // Build multipart/form-data request body from the parameter type's properties
            var schema = context.SchemaGenerator.GenerateSchema(upload.ParameterType, context.SchemaRepository);

            if (schema?.Properties == null)
            {
                return;
            }

            var properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>();

            foreach (var property in schema.Properties)
            {
                if (property.Value.Format == "binary" || property.Key.ToLower().Contains("file"))
                {
                    properties[property.Key] = new OpenApiSchema { Type = "string", Format = "binary" };
                }
                else
                {
                    properties[property.Key] = property.Value;
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties
                        }
                    }
                }
            };
        }

        #endregion
    }
}
