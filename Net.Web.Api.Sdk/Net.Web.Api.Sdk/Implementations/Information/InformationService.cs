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
        private readonly IWebHostEnvironment _environment;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationService"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        /// <param name="environment">The web host environment.</param>
        /// <exception cref="ArgumentNullException">tokenService</exception>
        public InformationService(IJwtTokenService tokenService, IWebHostEnvironment environment)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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

            // Gather referenced assembly information (replaces NuGet.Core PackageReferenceFile)
            var referencedAssemblies = GetReferencedAssemblies();
            if (referencedAssemblies.Any())
            {
                result.packages = referencedAssemblies;
            }

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

            result.title = title?.Title;
            result.description = description?.Description;
            result.version = version?.Version;
            result.copyright = copyright?.Copyright;
            result.author = author?.Company;
            result.license = Resources.License;

            return result;
        }

        /// <summary>
        /// Gets the referenced assemblies information using reflection.
        /// </summary>
        /// <returns>List of dynamic objects with assembly info.</returns>
        private static List<dynamic> GetReferencedAssemblies()
        {
            var result = new List<dynamic>();
            var executingAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = executingAssembly.GetReferencedAssemblies();

            foreach (var assemblyName in referencedAssemblies.OrderBy(a => a.Name))
            {
                dynamic packageInfo = new ExpandoObject();
                packageInfo.name = assemblyName.Name;
                packageInfo.version = assemblyName.Version?.ToString() ?? "unknown";
                result.Add(packageInfo);
            }

            return result;
        }

        #endregion
    }
}
