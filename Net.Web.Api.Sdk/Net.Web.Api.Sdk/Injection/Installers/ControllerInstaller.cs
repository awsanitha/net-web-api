using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <inheritdoc />
    /// <summary>
    /// Class ControllerInstaller.
    /// </summary>
    public class ControllerInstaller : IWindsorInstaller
    {
        #region IWindsorInstaller Implementations

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyInDirectory(new AssemblyFilter(AppDomain.CurrentDomain.BaseDirectory))
                .BasedOn<ControllerBase>()
                .LifestyleTransient());
        }

        #endregion
    }
}
