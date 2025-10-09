using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Scopes
{
    /// <summary>
    /// Class WindsorDependencyScope.
    /// Provides scoped service resolution using Castle Windsor for ASP.NET Core
    /// </summary>
    public class WindsorDependencyScope : IServiceScope
    {
        #region Private Properties

        /// <summary>
        /// The container
        /// </summary>
        private readonly IWindsorContainer _container;

        /// <summary>
        /// The scope
        /// </summary>
        private readonly IDisposable _scope;

        /// <summary>
        /// The service provider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyScope"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorDependencyScope(IWindsorContainer container)
        {
            _container = container;
            _scope = container.BeginScope();
            _serviceProvider = new WindsorServiceProvider(container);
        }

        #endregion

        #region IServiceScope Implementations

        /// <inheritdoc />
        /// <summary>
        /// Gets the service provider for this scope.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _scope?.Dispose();
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Windsor service provider for scoped resolution.
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
                    return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : null;
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
