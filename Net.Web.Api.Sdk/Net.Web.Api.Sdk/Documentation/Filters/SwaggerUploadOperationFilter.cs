using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    /// <seealso cref="T:Web.Api.Toolkit.Swashbuckle.Swagger.IOperationFilter" />
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
            var apiDescription = context.ApiDescription;
            var upload = apiDescription.ActionDescriptor?.EndpointMetadata
                .OfType<SwaggerUploadOperationAttribute>().FirstOrDefault();

            if (upload == null)
            {
                return;
            }

            // Check if schema exists in repository
            if (!context.SchemaRepository.TryLookupByType(upload.ParameterType, out _))
            {
                return;
            }

            // Clear any existing parameters
            operation.Parameters?.Clear();

            // Set up multipart/form-data content type
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    }
                }
            };

            var formSchema = operation.RequestBody.Content["multipart/form-data"].Schema;

            // Add file upload parameters
            // Note: This is a simplified implementation - you'll need to adjust according to your actual schema properties
            formSchema.Properties["file"] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "File to upload"
            };
            formSchema.Required.Add("file");
        }

        #endregion
    }
}