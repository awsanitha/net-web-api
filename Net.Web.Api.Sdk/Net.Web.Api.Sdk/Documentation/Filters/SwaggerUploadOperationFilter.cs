using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// Configures multipart/form-data upload operations.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), false)
                .Cast<SwaggerUploadOperationAttribute>()
                .FirstOrDefault();

            if (upload == null)
            {
                return;
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
                            Properties = GetPropertiesFromType(upload.ParameterType, context.SchemaRepository)
                        }
                    }
                }
            };
        }

        private static System.Collections.Generic.Dictionary<string, OpenApiSchema> GetPropertiesFromType(
            System.Type type, SchemaRepository schemaRepository)
        {
            var properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>();

            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType == typeof(IFormFile))
                {
                    properties[prop.Name.ToLower()] = new OpenApiSchema { Type = "string", Format = "binary" };
                }
                else
                {
                    properties[prop.Name.ToLower()] = new OpenApiSchema { Type = "string" };
                }
            }

            return properties;
        }
    }
}
