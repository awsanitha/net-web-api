using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Implementations.Information
{
    /// <summary>
    /// Class InformationService.
    /// Implements the <see cref="IInformationService" />
    /// </summary>
    public sealed class InformationService : IInformationService
    {
        #region Services

        private readonly IJwtTokenService _tokenService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationService"/> class.
        /// </summary>
        public InformationService(IJwtTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region IInformationService Implementations

        /// <summary>
        /// Gets the SDK informations.
        /// </summary>
        public dynamic GetSdkInformations()
        {
            dynamic result = new ExpandoObject();

            result.library = GetAssemblyInformations();

            var tokens = _tokenService.Tokens.Select(c => c.Value).ToList().OrderBy(c => c.TokenName);

            result.availableTokens = tokens;

            return result;
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
