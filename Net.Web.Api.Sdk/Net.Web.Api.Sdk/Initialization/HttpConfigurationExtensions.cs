using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Extension methods for registering and configuring the Web API SDK.
    /// </summary>
    public static class WebApiSdkExtensions
    {
        #region Private Constants

        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";
        private const string KIT_INDEX = "swagger.html";

        #endregion

        #region IServiceCollection Extensions

        /// <summary>
        /// Registers the Web API SDK services into the ASP.NET Core DI container.
        /// Call this from <c>builder.Services.AddWebApiSdk()</c> in Program.cs.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">Optional assembly name prefix for service scanning.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            IdentityModelEventSource.ShowPII = true;

            // Register application services by convention
            var installer = new ServiceInstaller(assemblyNamePrefix);
            installer.Install(services);

            // Register IHttpContextAccessor so services can access the HTTP context
            services.AddHttpContextAccessor();

            // Register MVC controllers with Newtonsoft.Json
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Register CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Register API versioning
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Register Swagger/OpenAPI
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                c.DocumentFilter<SwaggerMethodOrderingFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();

                c.EnableAnnotations();

                var basePath = AppContext.BaseDirectory;
                foreach (var file in Directory.GetFiles(basePath, "doc-api-*.xml"))
                {
                    c.IncludeXmlComments(file, includeControllerXmlComments: true);
                }
            });

            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }

        #endregion

        #region IApplicationBuilder Extensions

        /// <summary>
        /// Configures the Web API SDK middleware pipeline.
        /// Call this from <c>app.UseWebApiSdk()</c> in Program.cs after building the app.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseWebApiSdk(this IApplicationBuilder app)
        {
            // Store the service provider in the InjectionContainer for backward compatibility
            InjectionContainer.Instance.SetServiceProvider(app.ApplicationServices);

            app.UseCors();

            // Use the JWT token middleware (replaces DelegatingHandler)
            app.UseMiddleware<JwtTokenMiddleware>();

            // Configure Swagger UI
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "{documentName}/swagger/swagger.json";
            });

            var apiVersionDescriptionProvider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

            app.UseSwaggerUI(c =>
            {
                if (apiVersionDescriptionProvider != null)
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        c.SwaggerEndpoint(
                            $"/{description.GroupName}/swagger/swagger.json",
                            $"Version {description.ApiVersion}");
                    }
                }

                var assembly = Assembly.GetExecutingAssembly();
                c.IndexStream = () => assembly.GetManifestResourceStream(
                    $"{EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE}.index.html");
            });

            // Extract embedded resources to disk
            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
            if (env != null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                ExtractTextResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE,
                    KIT_DEFAULT_TOKEN_CONFIGURATION, KIT_DEFAULT_TOKEN_CONFIGURATION, env.ContentRootPath);
                ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                    KIT_SWAGGER_CONFIGURATION, KIT_SWAGGER_CONFIGURATION, env.ContentRootPath);
                ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                    KIT_DOCUMENTATION, KIT_DOCUMENTATION, env.ContentRootPath);
            }

            // Cleanup token database on startup
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IJwtTokenService>();
                service?.CleanupTokenDatabase();
            }

            return app;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts an embedded text resource to a file on disk.
        /// </summary>
        private static void ExtractTextResource(Assembly assembly, string nameSpace, string source, string destination, string rootPath)
        {
            var sourceResource = $"{nameSpace}.{source}";

            string content;

            using (var stream = assembly.GetManifestResourceStream(sourceResource))
            {
                if (stream == null)
                {
                    return;
                }

                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var fileName = Path.Combine(rootPath, destination);

            File.WriteAllText(fileName, content);
        }

        #endregion
    }
}
