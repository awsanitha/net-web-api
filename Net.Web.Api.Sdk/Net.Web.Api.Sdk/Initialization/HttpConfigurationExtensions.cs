using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Injection.Compositions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Injection.Resolvers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class WebApiSdkExtensions - ASP.NET Core service and app configuration extensions.
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
        /// Registers the Web API SDK services into the DI container.
        /// </summary>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services, string rootPath, string baseUrl = null)
        {
            IdentityModelEventSource.ShowPII = true;

            // Setup Windsor container
            var container = new WindsorContainer();

            container.Install(new ControllerInstaller());
            container.Install(new ServiceInstaller());

            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));

            InjectionContainer.Instance.SetContainer(container);

            // Register Windsor as controller activator
            services.AddSingleton<Microsoft.AspNetCore.Mvc.Controllers.IControllerActivator>(
                new WindsorCompositionRoot(container));

            // Register JWT token middleware
            services.AddTransient<JwtTokenHandler>();

            // Register validation filter
            services.AddControllers(options =>
            {
                options.Filters.Add<Common.Validations.ParameterValidationActionFilterAttribute>();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });

            // API versioning
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

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                c.DocumentFilter<SwaggerMethodOrderingFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var files = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var file in files)
                {
                    c.IncludeXmlComments(file);
                }
            });

            // Extract embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            ExtractTextResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, rootPath, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, rootPath, KIT_SWAGGER_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, rootPath, KIT_DOCUMENTATION);

            // Cleanup token database
            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
            service?.CleanupTokenDatabase();

            return services;
        }

        /// <summary>
        /// Configures the Web API SDK middleware pipeline.
        /// </summary>
        public static IApplicationBuilder UseWebApiSdk(this IApplicationBuilder app)
        {
            app.UseCors();
            app.UseMiddleware<JwtTokenHandler>();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }

        /// <summary>
        /// Disposes the Windsor container.
        /// </summary>
        public static void DisposeWebApiSdk()
        {
            InjectionContainer.Instance.DisposeContainer();
        }

        #endregion

        #region Private Methods

        private static void ExtractTextResource(Assembly assembly, string nameSpace, string source, string rootPath, string destin)
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

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var fileName = Path.Combine(rootPath, destin);

            File.WriteAllText(fileName, content);
        }

        #endregion
    }
}
