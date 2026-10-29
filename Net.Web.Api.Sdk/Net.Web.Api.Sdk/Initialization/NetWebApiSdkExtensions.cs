using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Environment;
using Net.Web.Api.Sdk.Common.Http;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Injection.Compositions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Injection.Scopes;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using Net.Web.Api.Sdk.Security.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class NetWebApiSdkExtensions.
    /// Replacement of the Web API <c>HttpConfiguration.RegisterWebApi</c> / <c>UnRegisterWebApi</c>
    /// pair: <see cref="AddNetWebApiSdk" /> registers the SDK services and
    /// <see cref="UseNetWebApiSdk" /> wires the request pipeline.
    /// </summary>
    public static class NetWebApiSdkExtensions
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

        /// <summary>
        /// The swagger user interface override stylesheet
        /// </summary>
        private const string KIT_UI_STYLESHEET = "swagger-ui-override.css";

        /// <summary>
        /// The swagger user interface override script
        /// </summary>
        private const string KIT_UI_SCRIPT = "swagger-ui-override.js";

        /// <summary>
        /// The swagger user interface route prefix
        /// </summary>
        private const string SWAGGER_ROUTE_PREFIX = "swagger";

        #endregion

        #region Public Extensions

        /// <summary>
        /// Registers every service, filter, versioning and documentation feature of the SDK.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The host environment.</param>
        /// <param name="serializerSettings">The JSON serializer settings used by the SDK responses.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddNetWebApiSdk(this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            JsonSerializerSettings serializerSettings = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            SdkEnvironment.Initialize(environment?.ContentRootPath, environment?.WebRootPath, configuration);
            Settings.Bind(configuration);

            IdentityModelEventSource.ShowPII = true;

            services.AddHttpContextAccessor();

            services.SetupInjection();
            services.SetupApi(serializerSettings);
            services.SetupDocumentation();

            return services;
        }

        /// <summary>
        /// Wires the SDK request pipeline and publishes the embedded resources the SDK ships with.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>WebApplication.</returns>
        public static WebApplication UseNetWebApiSdk(this WebApplication app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            PublishEmbeddedResources();

            app.UseStaticFiles();
            app.UseCors(SdkCorsConstants.DEFAULT_POLICY_NAME);
            app.UseMiddleware<JwtTokenHandler>();

            // Replaces the Windsor per-web-request lifestyle: controllers are registered with a
            // scoped lifestyle, so every request runs inside its own Windsor lifetime scope.
            app.Use(async (context, next) =>
            {
                using (new WindsorDependencyScope(InjectionContainer.Instance.Container))
                {
                    await next(context);
                }
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/{SWAGGER_ROUTE_PREFIX}/{description.GroupName}/swagger.json", description.GroupName);
                }

                options.RoutePrefix = SWAGGER_ROUTE_PREFIX;
                options.InjectStylesheet($"/{KIT_UI_STYLESHEET}");
                options.InjectJavascript($"/{KIT_UI_SCRIPT}");
                options.EnableValidator(null);
            });

            app.MapControllers();

            var service = InjectionContainer.Instance.GetService<IJwtTokenService>();

            service?.CleanupTokenDatabase();

            app.Lifetime.ApplicationStopping.Register(() => InjectionContainer.Instance.DisposeContainer());

            return app;
        }

        #endregion

        #region Private Extensions

        /// <summary>
        /// Setups the injection: the Windsor container keeps owning the SDK component discovery and
        /// every component it holds is exposed to the ASP.NET Core container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        private static void SetupInjection(this IServiceCollection services)
        {
            var container = new WindsorContainer();

            container.Register(Component.For<IHttpContextAccessor>().Instance(new HttpContextAccessor()));

            container.Install(new ControllerInstaller());
            container.Install(new ServiceInstaller());

            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));

            InjectionContainer.Instance.SetContainer(container);

            services.AddSingleton<IControllerActivator>(new WindsorCompositionRoot(container));

            BridgeComponents(services, container);
        }

        /// <summary>
        /// Setups the API: formatters, model validation, CORS and versioning.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        private static void SetupApi(this IServiceCollection services, JsonSerializerSettings serializerSettings)
        {
            services.AddCors(options => options.AddPolicy(SdkCorsConstants.DEFAULT_POLICY_NAME, policy => policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

            services
                .AddControllers(options => options.Filters.Add(new ParameterValidationActionFilterAttribute(serializerSettings)))
                .AddApplicationPart(typeof(NetWebApiSdkExtensions).Assembly)
                .AddNewtonsoftJson(options =>
                {
                    var settings = serializerSettings ?? ResponseActionResult.DefaultSerializerSettings;

                    options.SerializerSettings.DateFormatHandling = settings.DateFormatHandling;
                    options.SerializerSettings.DateTimeZoneHandling = settings.DateTimeZoneHandling;
                    options.SerializerSettings.ContractResolver = settings.ContractResolver ?? new CamelCasePropertyNamesContractResolver();
                });

            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services
                .AddApiVersioning(options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                })
                .AddMvc()
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });
        }

        /// <summary>
        /// Setups the documentation.
        /// </summary>
        /// <param name="services">The service collection.</param>
        private static void SetupDocumentation(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Exposes every component held by the Windsor container to the ASP.NET Core container, so a
        /// single instance source backs both containers.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="container">The container.</param>
        private static void BridgeComponents(IServiceCollection services, IWindsorContainer container)
        {
            var handlers = container.Kernel.GetAssignableHandlers(typeof(object));

            foreach (var handler in handlers)
            {
                foreach (var serviceType in handler.ComponentModel.Services)
                {
                    if (typeof(ControllerBase).IsAssignableFrom(serviceType) || serviceType == typeof(IHttpContextAccessor))
                    {
                        continue;
                    }

                    var resolvedType = serviceType;

                    services.AddSingleton(resolvedType, _ => container.Resolve(resolvedType));
                }
            }
        }

        /// <summary>
        /// Writes the resources embedded in the SDK assembly to disk: the configuration and
        /// documentation files next to the application and the Swagger assets under the web root.
        /// </summary>
        private static void PublishEmbeddedResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var rootPath = SdkEnvironment.RootPath;
            var webRootPath = SdkEnvironment.WebRootPath;

            ExtractResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, rootPath, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, rootPath, KIT_SWAGGER_CONFIGURATION);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, rootPath, KIT_DOCUMENTATION);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_INDEX, webRootPath, KIT_INDEX);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_UI_STYLESHEET, webRootPath, KIT_UI_STYLESHEET);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_UI_SCRIPT, webRootPath, KIT_UI_SCRIPT);
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, "logo-16x16.png", webRootPath, "logo-16x16.png");
            ExtractResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, "logo-32x32.png", webRootPath, "logo-32x32.png");
        }

        /// <summary>
        /// Extracts an embedded resource to a destination directory.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="source">The source.</param>
        /// <param name="destinationDirectory">The destination directory.</param>
        /// <param name="destin">The destination file name.</param>
        private static void ExtractResource(Assembly assembly, string nameSpace, string source, string destinationDirectory, string destin)
        {
            var sourceResource = $"{nameSpace}.{source}";

            using (var stream = assembly.GetManifestResourceStream(sourceResource))
            {
                if (stream == null)
                {
                    return;
                }

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                var fileName = Path.Combine(destinationDirectory, destin);

                using (var file = File.Create(fileName))
                {
                    stream.CopyTo(file);
                }
            }
        }

        #endregion
    }
}
