using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), false)
                .Cast<SwaggerUploadOperationAttribute>()
                .FirstOrDefault();

            if (upload == null)
            {
                return;
            }

            var parameterType = upload.ParameterType;
            var properties = parameterType.GetProperties();

            operation.Parameters.Clear();
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>()
                        }
                    }
                }
            };

            var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

            foreach (var property in properties)
            {
                var propertyName = property.Name.ToLowerInvariant();
                
                if (property.PropertyType == typeof(Microsoft.AspNetCore.Http.IFormFile) || 
                    property.PropertyType.Name.Contains("File"))
                {
                    schema.Properties[propertyName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = $"Upload {property.Name}"
                    };
                }
                else
                {
                    schema.Properties[propertyName] = new OpenApiSchema
                    {
                        Type = GetOpenApiType(property.PropertyType),
                        Description = property.Name
                    };
                }
            }
        }

        private static string GetOpenApiType(System.Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int) || type == typeof(long)) return "integer";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "number";
            return "string";
        }

        #endregion
    }
}
