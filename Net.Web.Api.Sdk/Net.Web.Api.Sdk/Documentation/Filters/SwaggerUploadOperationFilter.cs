using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    public class SwaggerUploadOperationFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var upload = context.MethodInfo?.GetCustomAttribute<SwaggerUploadOperationAttribute>(false);

            if (upload == null)
            {
                return;
            }

            // Replace parameters with a multipart/form-data file upload
            operation.Parameters.Clear();
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
                            },
                            Required = new System.Collections.Generic.HashSet<string> { "fileInformation" }
                        }
                    }
                }
            };
        }

        #endregion
    }
}
