using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models;
using Net.Web.Api.Sdk.Models.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class WebApplicationExtensions.
    /// Provides extension methods for configuring the SDK services and middleware.
    /// </summary>
    public static class WebApplicationExtensions
    {
        #region Private Constants

        /// <summary>
        /// The kit swagger configuration
        /// </summary>
        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";

        /// <summary>
        /// The kit default token configuration
        /// </summary>
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";

        /// <summary>
        /// The kit documentation
        /// </summary>
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";

        /// <summary>
        /// The kit index
        /// </summary>
        private const string KIT_INDEX = "swagger.html";

        #endregion

        #region Public Extensions

        /// <summary>
        /// Adds SDK services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optional action to configure upload file options.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddSdkServices(this IServiceCollection services, Action<UploadFileOptions> configureOptions = null)
        {
            IdentityModelEventSource.ShowPII = true;

            // Register HTTP context accessor
            services.AddHttpContextAccessor();

            // Register services via assembly scanning
            services.AddSdkServices(assemblyNamePrefix: null);

            // Configure upload file options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<UploadFileOptions>(_ => { });
            }

            // Configure Newtonsoft.Json serialization
            services.AddControllers(options =>
            {
                options.Filters.Add<ParameterValidationActionFilterAttribute>();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            // Configure API versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Configure Swagger
            services.AddSwaggerGen(swaggerDocConfig =>
            {
                swaggerDocConfig.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "API",
                    Version = "v1",
                    Description = "Version 1.0"
                });

                swaggerDocConfig.OperationFilter<SwaggerConsumesFilter>();
                swaggerDocConfig.OperationFilter<SwaggerProducesFilter>();
                swaggerDocConfig.OperationFilter<SwaggerUploadOperationFilter>();
                swaggerDocConfig.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                swaggerDocConfig.DocumentFilter<SwaggerMethodOrderingFilter>();
                swaggerDocConfig.DocumentFilter<SwaggerOperationOrderingFilter>();

                var basePath = AppContext.BaseDirectory;
                var files = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var file in files)
                {
                    swaggerDocConfig.IncludeXmlComments(file);
                }
            });

            // Register JwtTokenHandler as middleware (IMiddleware requires singleton registration)
            services.AddSingleton<JwtTokenHandler>();

            return services;
        }

        /// <summary>
        /// Uses SDK middleware in the application pipeline.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>WebApplication.</returns>
        public static WebApplication UseSdkMiddleware(this WebApplication app)
        {
            // Set static content root path for JwtTokenModel certificate resolution
            JwtTokenModel.ContentRootPath = app.Environment.ContentRootPath;

            // Extract embedded resources to content root
            ExtractEmbeddedResources(app.Environment.ContentRootPath);

            // Set up the InjectionContainer singleton with the service provider
            InjectionContainer.Instance.SetServiceProvider(app.Services);

            // Wire JWT token handler middleware
            app.UseMiddleware<JwtTokenHandler>();

            // Configure Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream($"{EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE}.index.html");
                c.InjectStylesheet("/swagger-ui-override.css");
                c.InjectJavascript("/swagger-ui-override.js");
            });

            // Serve extracted Swagger UI customization assets as static files
            app.UseStaticFiles();

            // Cleanup token database on startup
            using (var scope = app.Services.CreateScope())
            {
                var tokenService = scope.ServiceProvider.GetService<IJwtTokenService>();
                tokenService?.CleanupTokenDatabase();
            }

            return app;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts embedded resources to the content root.
        /// </summary>
        /// <param name="contentRootPath">The content root path.</param>
        private static void ExtractEmbeddedResources(string contentRootPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNamespace = EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE;
            var securityNamespace = EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE;

            ExtractTextResource(assembly, resourceNamespace, KIT_SWAGGER_CONFIGURATION, KIT_SWAGGER_CONFIGURATION, contentRootPath);
            ExtractTextResource(assembly, resourceNamespace, KIT_DOCUMENTATION, KIT_DOCUMENTATION, contentRootPath);
            ExtractTextResource(assembly, resourceNamespace, KIT_INDEX, "swagger.html", contentRootPath);
            ExtractTextResource(assembly, securityNamespace, KIT_DEFAULT_TOKEN_CONFIGURATION, KIT_DEFAULT_TOKEN_CONFIGURATION, contentRootPath);
        }

        /// <summary>
        /// Extracts the text resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="source">The source.</param>
        /// <param name="destin">The destin.</param>
        /// <param name="rootPath">The root path.</param>
        private static void ExtractTextResource(Assembly assembly, string nameSpace, string source, string destin, string rootPath)
        {
            var sourceResource = $"{nameSpace}.{source}";

            var content = string.Empty;

            using (var stream = assembly.GetManifestResourceStream(sourceResource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = reader.ReadToEnd();
                    }
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                var fileName = Path.Combine(rootPath, destin);

                File.WriteAllText(fileName, content);
            }
        }

        #endregion
    }
}
