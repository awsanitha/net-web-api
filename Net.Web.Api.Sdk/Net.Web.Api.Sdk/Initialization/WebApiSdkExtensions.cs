using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
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
using Net.Web.Api.Sdk.Middleware;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Extension methods to register and configure the Web API SDK in an ASP.NET Core application.
    /// </summary>
    public static class WebApiSdkExtensions
    {
        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";

        /// <summary>
        /// Registers all SDK services, versioning, Swagger and CORS into <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddHttpContextAccessor();
            services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                });

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

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerConsumesFilter>();
                options.OperationFilter<SwaggerProducesFilter>();
                options.OperationFilter<SwaggerUploadOperationFilter>();
                options.OperationFilter<SwaggerSecurityTypeAttributeFilter>();
                options.DocumentFilter<SwaggerMethodOrderingFilter>();
                options.DocumentFilter<SwaggerOperationOrderingFilter>();

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var file in Directory.GetFiles(basePath, "doc-api-*.xml"))
                {
                    options.IncludeXmlComments(file);
                }
            });
            services.AddSwaggerGenNewtonsoftSupport();

            // Register SDK services via assembly scanning
            services.InstallServices(assemblyNamePrefix);

            return services;
        }

        /// <summary>
        /// Configures the SDK middleware, routing, Swagger UI and extracts embedded resources.
        /// </summary>
        public static WebApplication UseWebApiSdk(this WebApplication app)
        {
            InjectionContainer.Instance.SetServiceProvider(app.Services);

            app.UseCors();
            app.UseMiddleware<JwtTokenMiddleware>();
            app.UseRouting();
            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName);
                }
                c.InjectJavascript("/swagger-ui-override.js");
                c.InjectStylesheet("/swagger-ui-override.css");
            });

            var env = app.Services.GetRequiredService<IWebHostEnvironment>();
            ExtractEmbeddedResource(EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, env.ContentRootPath);
            ExtractEmbeddedResource(EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, env.ContentRootPath);
            ExtractEmbeddedResource(EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, env.ContentRootPath);

            var service = app.Services.GetService<IJwtTokenService>();
            service?.CleanupTokenDatabase();

            return app;
        }

        private static void ExtractEmbeddedResource(string nameSpace, string resourceName, string destDirectory)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"{nameSpace}.{resourceName}";

            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null) return;

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            var destFile = Path.Combine(destDirectory, resourceName);
            File.WriteAllText(destFile, content);
        }
    }
}
