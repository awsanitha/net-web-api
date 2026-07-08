using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Documentation.Filters;
using Net.Web.Api.Sdk.Implementations.File;
using Net.Web.Api.Sdk.Implementations.Information;
using Net.Web.Api.Sdk.Implementations.Token;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Resolvers;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class ServiceCollectionExtensions. Provides registration of SDK services for ASP.NET Core.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Adds SDK Web API services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">Optional prefix for assembly scanning.</param>
        public static IServiceCollection AddSdkWebApi(this IServiceCollection services, string? assemblyNamePrefix = null)
        {
            IdentityModelEventSource.ShowPII = true;

            // Register core infrastructure
            services.AddHttpContextAccessor();

            // Register SDK built-in services
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IInformationService, InformationService>();
            services.AddSingleton<IFileService, FileService>();

            // Register application services from assemblies via [InjectInterfaceService] scanning
            services.RegisterSdkServices(assemblyNamePrefix);

            // Configure MVC with Newtonsoft.Json
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

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Configure Swagger
            services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
                options.DescribeAllParametersInCamelCase();

                options.OperationFilter<SwaggerConsumesFilter>();
                options.OperationFilter<SwaggerProducesFilter>();
                options.OperationFilter<SwaggerUploadOperationFilter>();
                options.OperationFilter<SwaggerSecurityTypeAttributeFilter>();

                options.DocumentFilter<SwaggerMethodOrderingFilter>();
                options.DocumentFilter<SwaggerOperationOrderingFilter>();

                // Include XML documentation if available
                var basePath = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
                var xmlFiles = Directory.GetFiles(basePath, "doc-api-*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    options.IncludeXmlComments(xmlFile);
                }

                // Include SDK XML doc embedded resource
                var sdkAssembly = Assembly.GetExecutingAssembly();
                var sdkXmlResource = $"{EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE}.doc-api-sdk.xml";

                using var stream = sdkAssembly.GetManifestResourceStream(sdkXmlResource);
                if (stream != null)
                {
                    var tempFile = Path.Combine(Path.GetTempPath(), "doc-api-sdk.xml");
                    using var writer = new FileStream(tempFile, FileMode.Create);
                    stream.CopyTo(writer);
                    options.IncludeXmlComments(tempFile);
                }
            })
            .AddSwaggerGenNewtonsoftSupport();

            return services;
        }

        /// <summary>
        /// Configures the SDK middleware pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static IApplicationBuilder UseSdkWebApi(this IApplicationBuilder app)
        {
            // Store service provider in the singleton container for attribute-based resolution
            InjectionContainer.Instance.SetServiceProvider(app.ApplicationServices);

            app.UseCors();

            // JWT token validation middleware
            app.UseMiddleware<JwtTokenMiddleware>();

            // Swagger UI setup
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "{documentName}/swagger/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/v1/swagger/swagger.json", "v1");
            });

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Cleanup token database on startup
            var tokenService = app.ApplicationServices.GetService<IJwtTokenService>();
            tokenService?.CleanupTokenDatabase();

            return app;
        }

        #endregion
    }
}
