using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes<SwaggerUploadOperationAttribute>().FirstOrDefault();

            if (upload == null)
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(upload.ParameterType, context.SchemaRepository);

            if (schema == null)
            {
                return;
            }

            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            var type = upload.ParameterType;

            foreach (var prop in type.GetProperties())
            {
                var propName = char.ToLower(prop.Name[0]) + prop.Name.Substring(1);

                if (typeof(IFormFile).IsAssignableFrom(prop.PropertyType))
                {
                    properties[propName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else
                {
                    properties[propName] = new OpenApiSchema
                    {
                        Type = "string"
                    };
                }

                if (prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true).Any())
                {
                    required.Add(propName);
                }
            }

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
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }

        #endregion
    }
}
