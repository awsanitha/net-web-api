using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerUploadOperationFilter.
    /// </summary>
    /// <seealso cref="T:Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
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
            var upload = SwaggerFilterHelper.GetControllerAndActionAttributes<SwaggerUploadOperationAttribute>(context).FirstOrDefault();

            if (upload == null)
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(upload.ParameterType, context.SchemaRepository);

            operation.Parameters?.Clear();

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [ConsumerProducerConstants.MULTIPART] = new OpenApiMediaType
                    {
                        Schema = schema
                    }
                }
            };
        }

        #endregion
    }
}
