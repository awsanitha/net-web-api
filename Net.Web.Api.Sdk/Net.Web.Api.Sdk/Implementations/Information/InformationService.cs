using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

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
        /// The nuget information file name
        /// </summary>
        private const string NUGET_INFORMATION_FILE_NAME = "packages.config";

        #endregion

        #region Services

        /// <summary>
        /// The token service
        /// </summary>
        private readonly IJwtTokenService _tokenService;

        /// <summary>
        /// The HTTP context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationService"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <exception cref="ArgumentNullException">tokenService</exception>
        public InformationService(IJwtTokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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

            // Parse packages.config from embedded resource using XML (replacing NuGet.Core)
            var assembly = Assembly.GetExecutingAssembly();
            var sourceResource = $"{assembly.GetName().Name}.{NUGET_INFORMATION_FILE_NAME}";

            using (var stream = assembly.GetManifestResourceStream(sourceResource))
            {
                if (stream != null)
                {
                    try
                    {
                        var doc = XDocument.Load(stream);
                        var packages = doc.Root?.Elements("package")
                            .Select(p => new
                            {
                                name = p.Attribute("id")?.Value,
                                version = p.Attribute("version")?.Value,
                                framework = p.Attribute("targetFramework")?.Value ?? "net10.0"
                            })
                            .ToList();

                        if (packages != null && packages.Count > 0)
                        {
                            result.packages = packages;
                        }
                    }
                    catch
                    {
                        // If parsing fails, skip package information
                    }
                }
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

            result.title = title?.Title ?? string.Empty;
            result.description = description?.Description ?? string.Empty;
            result.version = version?.Version ?? string.Empty;
            result.copyright = copyright?.Copyright ?? string.Empty;
            result.author = author?.Company ?? string.Empty;
            result.license = Resources.License;

            return result;
        }

        #endregion
    }
}
