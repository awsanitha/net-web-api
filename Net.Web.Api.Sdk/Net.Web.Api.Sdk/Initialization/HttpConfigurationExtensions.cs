using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Configurations.Token;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Documentation.Filters.Common;
using Net.Web.Api.Sdk.Implementations.File;
using Net.Web.Api.Sdk.Implementations.Information;
using Net.Web.Api.Sdk.Implementations.Token;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using Net.Web.Api.Sdk.Security.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class SdkApiExtensions.
    /// Provides extension methods for registering and configuring the SDK API services and middleware.
    /// Replaces the former HttpConfigurationExtensions which used System.Web.Http.HttpConfiguration.
    /// </summary>
    public static class SdkApiExtensions
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
        /// Adds SDK API services to the service collection.
        /// Registers DI services, configures JSON serialization (camel-case, Microsoft date format),
        /// registers API versioning, adds Swashbuckle with all migrated filters,
        /// and registers IJwtTokenService, IFileService, IInformationService, SdkUploadSettings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyPrefix">The assembly name prefix for scanning services.</param>
        /// <param name="configureControllers">Optional action to further configure MVC controllers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSdkApiServices(this IServiceCollection services, string assemblyPrefix = null, Action<MvcOptions> configureControllers = null)
        {
            // Configure JSON serialization (camelCase, Microsoft date format)
            var mvcBuilder = services.AddControllers(options =>
            {
                options.Filters.Add<ParameterValidationActionFilterAttribute>();
                configureControllers?.Invoke(options);
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            });

            // Register CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // Register API versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Register Swashbuckle with migrated filters
            services.AddSwaggerGen(c =>
            {
                c.DocumentFilter<SwaggerMethodOrderingFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();

                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                c.EnableAnnotations();

                // Include XML documentation files
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var files = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }
            });

            // Register HttpContextAccessor
            services.AddHttpContextAccessor();

            // Register SdkUploadSettings with defaults
            services.Configure<SdkUploadSettings>(options => { });

            // Register services as singletons
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IInformationService, InformationService>();

            // Show PII in identity model errors for debugging
            IdentityModelEventSource.ShowPII = true;

            // Scan assemblies for [InjectInterfaceService] marked interfaces
            services.AddSdkServices(assemblyPrefix);

            return services;
        }

        /// <summary>
        /// Configures the SDK API middleware pipeline.
        /// Adds JwtTokenMiddleware, enables CORS, maps Swagger UI,
        /// extracts embedded resources, and wires InjectionContainer.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseSdkApiMiddleware(this IApplicationBuilder app)
        {
            // Wire InjectionContainer for attributes that can't use constructor DI
            InjectionContainer.Instance.SetContainer(app.ApplicationServices);

            // Add JWT token middleware
            app.UseMiddleware<JwtTokenMiddleware>();

            // CORS
            app.UseCors();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // Extract embedded resources to content root
            ExtractEmbeddedResources(app);

            // Cleanup token database on startup
            try
            {
                var tokenService = app.ApplicationServices.GetService<IJwtTokenService>();
                tokenService?.CleanupTokenDatabase();
            }
            catch
            {
                // Token service may not be fully initialized if no token configs exist
            }

            return app;
        }

        /// <summary>
        /// Disposes the SDK API resources.
        /// </summary>
        public static void UnRegisterSdkApi()
        {
            // No-op: IServiceProvider lifecycle is managed by the host.
            // Retained for API compatibility with the original UnRegisterWebApi.
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts embedded resources (swagger config, documentation, token config) to the content root.
        /// </summary>
        /// <param name="app">The application builder.</param>
        private static void ExtractEmbeddedResources(IApplicationBuilder app)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var env = app.ApplicationServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            var rootPath = env?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;

            // Extract security resources (token config)
            ExtractTextResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, KIT_DEFAULT_TOKEN_CONFIGURATION, rootPath);

            // Extract documentation resources
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, KIT_SWAGGER_CONFIGURATION, rootPath);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, KIT_DOCUMENTATION, rootPath);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_INDEX, "swagger.html", rootPath);
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

            var fileName = Path.Combine(rootPath, destin);

            System.IO.File.WriteAllText(fileName, content);
        }

        #endregion
    }
}
