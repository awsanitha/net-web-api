using System;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Implementations.File;
using Net.Web.Api.Sdk.Implementations.Information;
using Net.Web.Api.Sdk.Implementations.Token;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class WebApiSdkExtensions — replaces HttpConfigurationExtensions for ASP.NET Core.
    /// </summary>
    public static class WebApiSdkExtensions
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

        #endregion

        #region Public Extensions

        /// <summary>
        /// Adds the Web API SDK services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            // HTTP context accessor required by services
            services.AddHttpContextAccessor();

            // SDK services
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IInformationService, InformationService>();
            services.AddScoped<IFileService, FileService>();

            // JWT token middleware (registered as transient for IMiddleware)
            services.AddTransient<JwtTokenHandler>();

            // MVC filters
            services.AddControllers(options =>
            {
                options.Filters.Add<ParameterValidationActionFilterAttribute>();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // API versioning
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

            // Swagger
            services.AddSwaggerGen(SetupSwaggerGen);

            // Extract embedded SDK resources to the output directory
            ExtractEmbeddedSdkResources();

            return services;
        }

        /// <summary>
        /// Uses the Web API SDK middleware pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder UseWebApiSdk(this IApplicationBuilder app)
        {
            app.UseCors();

            // JWT token validation middleware
            app.UseMiddleware<JwtTokenHandler>();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            // Cleanup expired tokens on startup
            using (var scope = ((IApplicationBuilder)app).ApplicationServices.CreateScope())
            {
                var tokenService = scope.ServiceProvider.GetService<IJwtTokenService>();
                tokenService?.CleanupTokenDatabase();
            }

            return app;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setups the swagger gen options.
        /// </summary>
        /// <param name="options">The options.</param>
        private static void SetupSwaggerGen(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "API",
                Version = "v1"
            });

            options.EnableAnnotations();

            // Include XML documentation files
            var basePath = AppContext.BaseDirectory;
            var files = Directory.GetFiles(basePath, "doc-api-*.xml");

            foreach (var file in files)
            {
                options.IncludeXmlComments(file);
            }
        }

        /// <summary>
        /// Extracts embedded SDK resources to the application base directory.
        /// </summary>
        private static void ExtractEmbeddedSdkResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

            ExtractTextResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, KIT_DEFAULT_TOKEN_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_SWAGGER_CONFIGURATION, KIT_SWAGGER_CONFIGURATION);
            ExtractTextResource(assembly, EmbeddedResourceConstants.RESOURCE_ASSEMBLY_NAMESPACE, KIT_DOCUMENTATION, KIT_DOCUMENTATION);
        }

        /// <summary>
        /// Extracts a text resource to the application base directory.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination file name.</param>
        private static void ExtractTextResource(Assembly assembly, string nameSpace, string source, string destination)
        {
            var sourceResource = $"{nameSpace}.{source}";
            var rootPath = AppContext.BaseDirectory;

            var content = string.Empty;

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
