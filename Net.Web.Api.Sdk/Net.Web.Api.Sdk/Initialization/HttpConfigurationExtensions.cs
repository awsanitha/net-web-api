using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class WebApiSdkExtensions.
    /// Provides extension methods to configure and use the Net.Web.Api.Sdk in ASP.NET Core applications.
    /// </summary>
    public static class WebApiSdkExtensions
    {
        #region Private Constants

        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";
        private const string KIT_INDEX = "swagger.html";

        #endregion

        #region Public Extensions

        /// <summary>
        /// Adds Net.Web.Api.Sdk services to the DI container.
        /// Call this in Program.cs / Startup.ConfigureServices.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">Optional prefix to filter assemblies for service scanning.</param>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            // Register HttpContextAccessor for services that need it
            services.AddHttpContextAccessor();

            // Register SDK services via attribute scanning
            var installer = new ServiceInstaller(assemblyNamePrefix);
            installer.Install(services);

            // Setup API versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Setup Swagger / Swashbuckle
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                c.DocumentFilter<SwaggerMethodOrderingFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();

                c.EnableAnnotations();

                // Include XML documentation files
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var xmlFiles = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var file in xmlFiles)
                {
                    c.IncludeXmlComments(file);
                }
            });

            return services;
        }

        /// <summary>
        /// Configures the Net.Web.Api.Sdk middleware pipeline.
        /// Call this in Program.cs / Startup.Configure.
        /// </summary>
        public static IApplicationBuilder UseWebApiSdk(this IApplicationBuilder app)
        {
            IdentityModelEventSource.ShowPII = true;

            // Store the service provider in the injection container for service-locator access
            InjectionContainer.Instance.SetContainer(app.ApplicationServices);

            // Add JWT token validation middleware
            app.UseMiddleware<JwtTokenHandler>();

            // Extract embedded SDK resources
            var assembly = Assembly.GetExecutingAssembly();
            var env = app.ApplicationServices.GetService<IHostEnvironment>();
            var rootPath = env?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;

            ExtractEmbeddedResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, rootPath, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractEmbeddedResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, rootPath, KIT_SWAGGER_CONFIGURATION);
            ExtractEmbeddedResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, rootPath, KIT_DOCUMENTATION);
            ExtractEmbeddedResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_INDEX, rootPath, "swagger.html");

            // Trigger token DB cleanup on startup
            var serviceProvider = app.ApplicationServices;

            try
            {
                var tokenService = serviceProvider.GetService<IJwtTokenService>();
                tokenService?.CleanupTokenDatabase();
            }
            catch
            {
                // Non-critical - ignore cleanup failures on startup
            }

            return app;
        }

        /// <summary>
        /// Un-registers the SDK (disposes resources).
        /// </summary>
        public static void UnRegisterWebApiSdk(this IApplicationBuilder app)
        {
            InjectionContainer.Instance.DisposeContainer();
        }

        #endregion

        #region Private Methods

        private static void ExtractEmbeddedResource(Assembly assembly, string nameSpace, string source, string rootPath, string destination)
        {
            var sourceResource = $"{nameSpace}.{source}";

            string content = null;

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

            if (content == null)
            {
                return;
            }

            var fileName = Path.Combine(rootPath, destination);

            File.WriteAllText(fileName, content);
        }

        #endregion
    }
}
