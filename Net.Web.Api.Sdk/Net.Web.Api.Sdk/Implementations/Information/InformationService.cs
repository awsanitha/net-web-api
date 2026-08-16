using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;

namespace Net.Web.Api.Sdk.Implementations.Information
{
    /// <inheritdoc />
    public sealed class InformationService : IInformationService
    {
        #region Services

        private readonly IJwtTokenService _tokenService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="InformationService"/>.
        /// </summary>
        public InformationService(IJwtTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region IInformationService

        /// <inheritdoc />
        public dynamic GetSdkInformations()
        {
            dynamic result = new ExpandoObject();

            result.library = GetAssemblyInformations();
            result.availableTokens = _tokenService.Tokens.Values
                .OrderBy(t => t.TokenName)
                .ToList();

            return result;
        }

        #endregion

        #region Private Methods

        private dynamic GetAssemblyInformations()
        {
            var assembly = Assembly.GetAssembly(GetType());
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            var author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();

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
