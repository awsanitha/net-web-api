using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace Net.Web.Api.Sdk.Implementations.Information
{
    /// <summary>
    /// Class InformationService.
    /// Implements the <see cref="IInformationService" />
    /// </summary>
    /// <seealso cref="IInformationService" />
    public sealed class InformationService : IInformationService
    {
        #region Private Constants

        /// <summary>
        /// The package information file name
        /// </summary>
        private const string PACKAGE_INFORMATION_FILE_NAME = "packages.info";

        #endregion

        #region Services

        /// <summary>
        /// The token service
        /// </summary>
        private readonly IJwtTokenService _tokenService;

        /// <summary>
        /// The web host environment
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationService"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        /// <param name="webHostEnvironment">The web host environment.</param>
        /// <exception cref="ArgumentNullException">tokenService</exception>
        public InformationService(IJwtTokenService tokenService, IWebHostEnvironment webHostEnvironment = null)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _webHostEnvironment = webHostEnvironment;
        }

        #endregion

        #region IInformationService Implementations

        /// <summary>
        /// Gets the SDK informations.
        /// </summary>
        /// <returns>dynamic.</returns>
        public dynamic GetSdkInformations()
        {           
            dynamic result = new ExpandoObject();

            result.library = GetAssemblyInformations();

            var tokens = _tokenService.Tokens.Select(c => c.Value).ToList().OrderBy(c => c.TokenName);
            result.availableTokens = tokens;

            // Get package information from project file or dependencies
            result.packages = GetPackageInformations();

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the assembly informations.
        /// </summary>
        /// <returns>dynamic.</returns>
        private dynamic GetAssemblyInformations()
        {
            var libraryAssembly = Assembly.GetAssembly(GetType());
            var description = libraryAssembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
            var copyright = libraryAssembly.GetCustomAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var title = libraryAssembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            var version = libraryAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
            var author = libraryAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;

            dynamic result = new ExpandoObject();

            result.title = title?.Title ?? "Net Web API SDK";
            result.description = description?.Description ?? "A comprehensive SDK for .NET Web API development";
            result.version = version?.Version ?? libraryAssembly.GetName().Version?.ToString() ?? "1.0.0";
            result.copyright = copyright?.Copyright ?? "Copyright © 2024";
            result.author = author?.Company ?? "SDK Author";
            result.license = GetLicenseText();

            return result;
        }

        /// <summary>
        /// Gets the package informations.
        /// </summary>
        /// <returns>List of package information.</returns>
        private List<dynamic> GetPackageInformations()
        {
            var packages = new List<dynamic>();

            try
            {
                // Get loaded assemblies and their versions
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .OrderBy(a => a.GetName().Name);

                foreach (var assembly in loadedAssemblies)
                {
                    var assemblyName = assembly.GetName();
                    
                    dynamic package = new ExpandoObject();
                    package.name = assemblyName.Name;
                    package.version = assemblyName.Version?.ToString() ?? "Unknown";
                    package.framework = assembly.ImageRuntimeVersion ?? "Unknown";

                    packages.Add(package);
                }
            }
            catch (Exception)
            {
                // If we can't get package information, return empty list
            }

            return packages;
        }

        /// <summary>
        /// Gets the license text.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetLicenseText()
        {
            try
            {
                return Resources.License;
            }
            catch
            {
                return "License information not available";
            }
        }

        #endregion
    }
}
