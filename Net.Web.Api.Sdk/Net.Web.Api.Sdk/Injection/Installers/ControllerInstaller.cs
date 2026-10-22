using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <summary>
    /// Class ControllerInstaller. Registers controllers from an assembly with IServiceCollection.
    /// </summary>
    public class ControllerInstaller
    {
        public void Install(IMvcBuilder mvcBuilder, Assembly assembly)
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
    }
}
