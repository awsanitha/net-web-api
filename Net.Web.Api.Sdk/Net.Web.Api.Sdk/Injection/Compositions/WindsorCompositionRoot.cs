using System;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Compositions
{
    /// <summary>
    /// Class WindsorCompositionRoot.
    /// Provides integration between Castle Windsor and ASP.NET Core DI
    /// </summary>
    public class WindsorCompositionRoot
    {
        #region Private Properties

        /// <summary>
        /// The container
        /// </summary>
        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorCompositionRoot"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorCompositionRoot(IWindsorContainer container)
        {
            _container = container;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the services for ASP.NET Core DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Windsor container as a service
            services.AddSingleton(_container);
            
            // Add a service factory that uses Windsor to resolve services
            services.AddTransient<IServiceProvider>(provider => new WindsorServiceProvider(_container));
        }

        /// <summary>
        /// Resolves a service from the Windsor container.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The resolved service.</returns>
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Resolves a service from the Windsor container.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The resolved service.</returns>
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        /// <summary>
        /// Releases a service instance.
        /// </summary>
        /// <param name="instance">The instance to release.</param>
        public void Release(object instance)
        {
            _container.Release(instance);
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Windsor service provider for ASP.NET Core integration.
        /// </summary>
        private class WindsorServiceProvider : IServiceProvider
        {
            private readonly IWindsorContainer _container;

            public WindsorServiceProvider(IWindsorContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                try
                {
                    return _container.Resolve(serviceType);
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
