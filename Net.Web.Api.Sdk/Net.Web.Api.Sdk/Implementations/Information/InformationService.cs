using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Net.Web.Api.Sdk.Interfaces.Information;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;

namespace Net.Web.Api.Sdk.Implementations.Information
{
    /// <inheritdoc />
    public sealed class InformationService : IInformationService
    {
        private readonly IJwtTokenService _tokenService;
        private readonly IWebHostEnvironment _env;

        public InformationService(IJwtTokenService tokenService, IWebHostEnvironment env)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public dynamic GetSdkInformations()
        {
            dynamic result = new ExpandoObject();
            result.library = GetAssemblyInformations();
            result.availableTokens = _tokenService.Tokens.Select(c => c.Value).OrderBy(c => c.TokenName);
            return result;
        }

        private dynamic GetAssemblyInformations()
        {
            var asm = Assembly.GetAssembly(GetType());
            var description = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var title = asm.GetCustomAttribute<AssemblyTitleAttribute>();
            var version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>();
            var author = asm.GetCustomAttribute<AssemblyCompanyAttribute>();

            dynamic result = new ExpandoObject();
            result.title = title?.Title;
            result.description = description?.Description;
            result.version = version?.Version;
            result.copyright = copyright?.Copyright;
            result.author = author?.Company;
            result.license = Resources.License;
            return result;
        }
    }
}
