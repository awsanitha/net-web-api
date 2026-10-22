using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Extension methods for registering and configuring the SDK with ASP.NET Core.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        #region Private Constants

        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";
        private const string KIT_INDEX = "swagger.html";

        #endregion

        #region IServiceCollection Extensions

        /// <summary>
        /// Registers all SDK services and configuration with the service collection.
        /// </summary>
        public static IServiceCollection AddSdk(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddHttpContextAccessor();
            services.AddTransient<JwtTokenHandler>();

            var installer = new ServiceInstaller(assemblyNamePrefix);
            var callingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .ToList();
            installer.Install(services, callingAssemblies);

            services
                .AddControllers(options =>
                {
                    options.Filters.Add<ParameterValidationActionFilterAttribute>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var xmlFiles = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile);
                }

                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();
            });

            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }

        #endregion

        #region IApplicationBuilder Extensions

        /// <summary>
        /// Configures the SDK middleware pipeline and extracts embedded resources.
        /// </summary>
        public static IApplicationBuilder UseSdk(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            InjectionContainer.Instance.SetContainer(app.ApplicationServices);

            app.UseMiddleware<JwtTokenHandler>();

            var sdkAssembly = Assembly.GetExecutingAssembly();

            ExtractEmbeddedResource(sdkAssembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, env.ContentRootPath, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractEmbeddedResource(sdkAssembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, env.ContentRootPath, KIT_SWAGGER_CONFIGURATION);
            ExtractEmbeddedResource(sdkAssembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, env.ContentRootPath, KIT_DOCUMENTATION);
            ExtractEmbeddedResource(sdkAssembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_INDEX, env.ContentRootPath, "swagger.html");

            var tokenService = app.ApplicationServices.GetService<IJwtTokenService>();
            tokenService?.CleanupTokenDatabase();

            return app;
        }

        /// <summary>
        /// Configures Swagger UI for versioned APIs.
        /// </summary>
        public static IApplicationBuilder UseSdkSwagger(this IApplicationBuilder app)
        {
            var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger(c => c.RouteTemplate = "{documentName}/swagger/swagger.json");

            app.UseSwaggerUI(c =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                {
                    var name = description.IsDeprecated
                        ? $"Version {description.ApiVersion} (Deprecated)"
                        : $"Version {description.ApiVersion}";

                    c.SwaggerEndpoint($"/{description.GroupName}/swagger/swagger.json", name);
                }
            });

            return app;
        }

        #endregion

        #region Private Methods

        private static void ExtractEmbeddedResource(Assembly assembly, string nameSpace, string source, string rootPath, string destination)
        {
            var sourceResource = $"{nameSpace}.{source}";
            var content = string.Empty;

            using (var stream = assembly.GetManifestResourceStream(sourceResource))
            {
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    content = reader.ReadToEnd();
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                var fileName = Path.Combine(rootPath, destination);
                System.IO.File.WriteAllText(fileName, content);
            }
        }

        #endregion
    }
}
