using System;
using System.IO;
using System.Reflection;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Injection.Compositions;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Injection.Resolvers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class ServiceCollectionExtensions.
    /// </summary>
    public static class ServiceCollectionExtensions
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

        #endregion

        #region Public Extensions

        /// <summary>
        /// Configures the SDK services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureSdk(this IServiceCollection services)
        {
            // Configure JSON serialization
            services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
                options.JsonSerializerOptions.WriteIndented = true;
            });

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Configure API versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            // Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Net Web API SDK", 
                    Version = "v1",
                    Description = "A comprehensive SDK for .NET Web API development"
                });

                // Add custom filters
                c.OperationFilter<SwaggerConsumesFilter>();
                c.OperationFilter<SwaggerProducesFilter>();
                c.OperationFilter<SwaggerUploadOperationFilter>();
                c.OperationFilter<SwaggerSecurityTypeAttributeFilter>();
                c.DocumentFilter<SwaggerOperationOrderingFilter>();

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        /// <summary>
        /// Configures the Windsor container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureWindsor(this IServiceCollection services)
        {
            var container = new Castle.Windsor.WindsorContainer();
            
            // Configure Windsor - simplified for .NET 8
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));

            // Install components
            container.Install(new ControllerInstaller());

            // Set up the container
            Net.Web.Api.Sdk.Injection.Containers.WindsorContainer.Instance = container;

            // Add Windsor to DI
            services.AddSingleton<IWindsorContainer>(container);
            services.AddSingleton<WindsorCompositionRoot>(new WindsorCompositionRoot(container));

            return services;
        }

        /// <summary>
        /// Configures the JWT token service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="tokenConfigurationName">Name of the token configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureJwtTokenService(this IServiceCollection services, string tokenConfigurationName = null)
        {
            // Enable detailed identity model logging in development
            IdentityModelEventSource.ShowPII = true;

            // Register JWT token service
            var container = Net.Web.Api.Sdk.Injection.Containers.WindsorContainer.Instance;
            if (container != null)
            {
                var tokenService = container.Resolve<IJwtTokenService>();
                services.AddSingleton(tokenService);
            }

            return services;
        }

        /// <summary>
        /// Configures validation filters.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection ConfigureValidationFilters(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<ParameterValidationActionFilterAttribute>();
            });

            return services;
        }

        #endregion
    }
}
