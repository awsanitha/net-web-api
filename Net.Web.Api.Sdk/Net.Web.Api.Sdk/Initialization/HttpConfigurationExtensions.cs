using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Extension methods for registering and using the SDK Web API pipeline.
    /// </summary>
    public static class WebApiSdkExtensions
    {
        #region Private Constants

        private const string KIT_SWAGGER_CONFIGURATION = "SwaggerConfigurationSdk.json";
        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";
        private const string KIT_DOCUMENTATION = "doc-api-sdk.xml";

        #endregion

        #region Public Extensions

        /// <summary>
        /// Registers all SDK services into the ASP.NET Core DI container and configures the pipeline.
        /// Call this from <c>Program.cs</c> before <c>builder.Build()</c>.
        /// </summary>
        public static IServiceCollection AddSdkWebApi(
            this IServiceCollection services,
            IWebHostEnvironment env)
        {
            IdentityModelEventSource.ShowPII = true;

            // --- Castle.Windsor container ---
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.Install(new ServiceInstaller());

            InjectionContainer.Instance.SetContainer(container);

            // --- ASP.NET Core controllers + Newtonsoft JSON ---
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // --- API versioning ---
            services
                .AddApiVersioning(o =>
                {
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                    o.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddMvc()
                .AddApiExplorer(o =>
                {
                    o.GroupNameFormat = "'v'VVV";
                    o.SubstituteApiVersionInUrl = true;
                });

            // --- CORS ---
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // --- Swagger / Swashbuckle ---
            services.AddSwaggerGen(swaggerDocConfig =>
            {
                swaggerDocConfig.EnableAnnotations();

                swaggerDocConfig.OperationFilter<Documentation.Filters.SwaggerConsumesFilter>();
                swaggerDocConfig.OperationFilter<Documentation.Filters.SwaggerProducesFilter>();
                swaggerDocConfig.OperationFilter<Documentation.Filters.SwaggerUploadOperationFilter>();
                swaggerDocConfig.OperationFilter<Documentation.Filters.SwaggerSecurityTypeAttributeFilter>();

                swaggerDocConfig.DocumentFilter<Documentation.Filters.SwaggerOperationOrderingFilter>();

                // Include all doc-api-*.xml files
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(basePath))
                {
                    foreach (var file in Directory.GetFiles(basePath, "doc-api-*.xml"))
                    {
                        swaggerDocConfig.IncludeXmlComments(file);
                    }
                }
            });
            services.AddSwaggerGenNewtonsoftSupport();

            // --- IHttpContextAccessor for services that need HttpContext ---
            services.AddHttpContextAccessor();

            // --- Extract embedded token configuration to disk ---
            var assembly = Assembly.GetExecutingAssembly();
            var contentRoot = env.ContentRootPath;
            ExtractTextResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE,
                KIT_DEFAULT_TOKEN_CONFIGURATION, contentRoot, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                KIT_SWAGGER_CONFIGURATION, contentRoot, KIT_SWAGGER_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                KIT_DOCUMENTATION, contentRoot, KIT_DOCUMENTATION);

            // --- Clean up expired tokens on startup ---
            var tokenService = InjectionContainer.Instance.GetService<IJwtTokenService>(contentRoot);
            tokenService?.CleanupTokenDatabase();

            return services;
        }

        /// <summary>
        /// Configures SDK middleware in the ASP.NET Core request pipeline.
        /// Call this from <c>Program.cs</c> after <c>builder.Build()</c>.
        /// </summary>
        public static IApplicationBuilder UseSdkWebApi(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseCors();
            app.UseRouting();

            // JWT token middleware (sets Thread.CurrentPrincipal / HttpContext.User)
            app.UseMiddleware<JwtTokenMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger UI
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;

                // Versioned endpoints are registered by Swashbuckle via ApiExplorer
                c.SwaggerEndpoint("/v1/swagger.json", "v1");

                // Inject custom UI overrides from embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                WriteResourceToWwwroot(assembly, env,
                    EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                    "swagger-ui-override.js");
                WriteResourceToWwwroot(assembly, env,
                    EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE,
                    "swagger-ui-override.css");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }

        /// <summary>
        /// Backward-compatible helper: un-registers the SDK (disposes the Windsor container).
        /// </summary>
        public static void UnRegisterWebApi()
        {
            InjectionContainer.Instance.DisposeContainer();
        }

        #endregion

        #region Private Helpers

        private static void ExtractTextResource(
            Assembly assembly,
            string nameSpace,
            string source,
            string targetDirectory,
            string destFileName)
        {
            var resourceName = $"{nameSpace}.{source}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var filePath = Path.Combine(targetDirectory, destFileName);
            File.WriteAllText(filePath, content);
        }

        private static void WriteResourceToWwwroot(
            Assembly assembly,
            IWebHostEnvironment env,
            string nameSpace,
            string resourceFileName)
        {
            var resourceName = $"{nameSpace}.{resourceFileName}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;

            var wwwroot = env.WebRootPath ?? env.ContentRootPath;
            var filePath = Path.Combine(wwwroot, resourceFileName);

            using var reader = new StreamReader(stream);
            File.WriteAllText(filePath, reader.ReadToEnd());
        }

        #endregion
    }
}
