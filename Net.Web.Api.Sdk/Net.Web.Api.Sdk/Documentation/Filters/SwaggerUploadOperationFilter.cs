using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes<SwaggerUploadOperationAttribute>(true).FirstOrDefault();

            if (upload == null)
            {
                return;
            }

            var parameterType = upload.ParameterType;
            var schemaKey = parameterType.Name;

            if (!context.SchemaRepository.Schemas.TryGetValue(schemaKey, out var schema))
            {
                // Generate schema if not already present
                schema = context.SchemaGenerator.GenerateSchema(parameterType, context.SchemaRepository);
            }

            // Build multipart/form-data schema properties from the parameter type's schema
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredProperties = new HashSet<string>();

            if (schema?.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    var name = property.Key;
                    var definition = property.Value;

                    // Check if property is an IFormFile (was HttpFile in old API)
                    var propInfo = parameterType.GetProperty(name,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var isFileType = propInfo != null &&
                        (typeof(Microsoft.AspNetCore.Http.IFormFile).IsAssignableFrom(propInfo.PropertyType) ||
                         propInfo.PropertyType.Name.Contains("File"));

                    if (isFileType || (definition.Reference != null && definition.Reference.Id != null &&
                        definition.Reference.Id.Contains("FormFile")))
                    {
                        properties.Add(name, new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = definition.Description
                        });
                    }
                    else
                    {
                        properties.Add(name, new OpenApiSchema
                        {
                            Type = definition.Type,
                            Description = definition.Description,
                            Default = definition.Default,
                            MaxLength = definition.MaxLength,
                            MinLength = definition.MinLength
                        });
                    }

                    if (schema.Required != null && schema.Required.Contains(name))
                    {
                        requiredProperties.Add(name);
                    }
                }
            }

            // Clear existing parameters that were auto-generated
            operation.Parameters?.Clear();

            // Set up multipart/form-data request body
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
                            Required = requiredProperties
                        }
                    }
                }
            };
        }

        #endregion
    }
}
