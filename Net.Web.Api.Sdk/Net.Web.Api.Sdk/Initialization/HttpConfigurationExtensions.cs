using System;
using System.IO;
using System.Reflection;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Common.Validations;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Injection.Installers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Handlers;

namespace Net.Web.Api.Sdk.Initialization
{
    /// <summary>
    /// Class WebApiSdkExtensions. Provides ASP.NET Core registration helpers for the SDK.
    /// </summary>
    public static class WebApiSdkExtensions
    {
        #region Private Constants

        private const string KIT_DEFAULT_TOKEN_CONFIGURATION = "token-sdk.config";

        #endregion

        #region Public Extensions

        /// <summary>
        /// Registers the SDK services into the Windsor container and sets up the injection container.
        /// Call this from your Program.cs / Startup before building the app.
        /// </summary>
        public static IServiceCollection AddWebApiSdk(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            IdentityModelEventSource.ShowPII = true;

            var container = new WindsorContainer();

            container.Install(new ControllerInstaller());
            container.Install(new ServiceInstaller(assemblyNamePrefix));

            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, true));

            InjectionContainer.Instance.SetContainer(container);

            // Extract the default token config to the base directory
            var assembly = Assembly.GetExecutingAssembly();
            ExtractEmbeddedResource(assembly, EmbeddedResourceConstants.SECURITY_ASSEMBLY_NAMESPACE, KIT_DEFAULT_TOKEN_CONFIGURATION, KIT_DEFAULT_TOKEN_CONFIGURATION);

            // Register the filter globally
            services.AddControllers(options =>
            {
                options.Filters.Add<ParameterValidationActionFilterAttribute>();
            });

            return services;
        }

        /// <summary>
        /// Registers the SDK middleware (JWT token handler) in the pipeline.
        /// Call this from your Program.cs after UseRouting and before UseAuthorization.
        /// </summary>
        public static IApplicationBuilder UseWebApiSdk(this IApplicationBuilder app)
        {
            app.UseMiddleware<JwtTokenMiddleware>();

            // Cleanup expired tokens on startup
            try
            {
                var service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                service?.CleanupTokenDatabase();
            }
            catch
            {
                // ignored - token service may not be configured
            }

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

        private static void ExtractEmbeddedResource(Assembly assembly, string nameSpace, string source, string destin)
        {
            var sourceResource = $"{nameSpace}.{source}";
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
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

            if (!string.IsNullOrEmpty(content))
            {
                var fileName = Path.Combine(rootPath, destin);
                File.WriteAllText(fileName, content);
            }
        }

        #endregion
    }
}
