using System;
using System.IO;
using System.Linq;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class ConfigureSwaggerOptions.
    /// Declares one Swagger document per discovered API version, which is what the Web API
    /// <c>MultipleApiVersions</c> call used to do.
    /// </summary>
    internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        #region Private Constants

        /// <summary>
        /// The documentation file pattern
        /// </summary>
        private const string DOCUMENTATION_FILE_PATTERN = "doc-api-*.xml";

        #endregion

        #region Private Properties

        /// <summary>
        /// The API version description provider
        /// </summary>
        private readonly IApiVersionDescriptionProvider _provider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The API version description provider.</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        #endregion

        #region IConfigureOptions Implementations

        /// <summary>
        /// Configures the Swagger generation options.
        /// </summary>
        /// <param name="options">The options.</param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"Version {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated ? "This Service version has been deprecated." : string.Empty
                });
            }

            options.EnableAnnotations();

            options.OperationFilter<SwaggerConsumesFilter>();
            options.OperationFilter<SwaggerProducesFilter>();
            options.OperationFilter<SwaggerUploadOperationFilter>();
            options.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

            options.DocumentFilter<SwaggerMethodOrderingFilter>();
            options.DocumentFilter<SwaggerOperationOrderingFilter>();

            foreach (var file in GetDocumentationFiles())
            {
                options.IncludeXmlComments(file);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the XML documentation files shipped next to the application.
        /// </summary>
        /// <returns>System.String[].</returns>
        private static string[] GetDocumentationFiles()
        {
            var basePath = AppContext.BaseDirectory;

            return Directory.Exists(basePath)
                ? Directory.GetFiles(basePath, DOCUMENTATION_FILE_PATTERN).ToArray()
                : Array.Empty<string>();
        }

        #endregion
    }
}
