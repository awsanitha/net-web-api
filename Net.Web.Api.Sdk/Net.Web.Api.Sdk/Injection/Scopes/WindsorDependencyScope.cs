using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Scopes
{
    /// <summary>
    /// Class WindsorDependencyScope - scoped service provider backed by Windsor.
    /// </summary>
    public class WindsorDependencyScope : IServiceScope, IServiceProvider
    {
        #region Private Properties

        private readonly IWindsorContainer _container;
        private readonly IDisposable _scope;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyScope"/> class.
        /// </summary>
        public WindsorDependencyScope(IWindsorContainer container)
        {
            _container = container;
            _scope = container.BeginScope();
        }

        #endregion

        #region IServiceScope Implementations

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => this;

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _scope.Dispose();
        }

        #endregion
    }
}
