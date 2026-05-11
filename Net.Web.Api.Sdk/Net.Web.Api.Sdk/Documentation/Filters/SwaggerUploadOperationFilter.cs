using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo.GetCustomAttributes(typeof(SwaggerUploadOperationAttribute), true)
                .FirstOrDefault() as SwaggerUploadOperationAttribute;

            if (upload == null)
            {
                return;
            }

            // Set up multipart/form-data request body for file upload
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties =
                            {
                                ["fileInformation"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "File to upload"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
