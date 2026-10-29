using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Net.Web.Api.Sdk.Common.Environment
{
    /// <summary>
    /// Class SdkEnvironment.
    /// Replaces the ASP.NET <c>HttpContext.Current.Server.MapPath</c> based path resolution and the
    /// <c>ConfigurationManager</c> based settings access with values supplied once at application start.
    /// </summary>
    public static class SdkEnvironment
    {
        #region Private Properties

        /// <summary>
        /// The root path
        /// </summary>
        private static string _rootPath;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the application root path. Equivalent of the legacy <c>Server.MapPath("\")</c>.
        /// </summary>
        /// <value>The root path.</value>
        public static string RootPath => _rootPath ?? AppContext.BaseDirectory;

        /// <summary>
        /// Gets the web root path used to publish uploaded and extracted files.
        /// </summary>
        /// <value>The web root path.</value>
        public static string WebRootPath { get; private set; }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public static IConfiguration Configuration { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the SDK environment. Must be called once during application start up.
        /// </summary>
        /// <param name="contentRootPath">The content root path.</param>
        /// <param name="webRootPath">The web root path.</param>
        /// <param name="configuration">The configuration.</param>
        public static void Initialize(string contentRootPath, string webRootPath, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(contentRootPath))
            {
                _rootPath = contentRootPath;
            }

            WebRootPath = string.IsNullOrEmpty(webRootPath)
                ? Path.Combine(RootPath, "wwwroot")
                : webRootPath;

            Configuration = configuration;
        }

        #endregion
    }
}
