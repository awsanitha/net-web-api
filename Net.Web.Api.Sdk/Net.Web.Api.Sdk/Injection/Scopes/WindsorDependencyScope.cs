using System;
using System.Collections.Generic;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;

namespace Net.Web.Api.Sdk.Injection.Scopes
{
    /// <summary>
    /// Class WindsorDependencyScope.
    /// </summary>
    public class WindsorDependencyScope : IDisposable
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

        #region Public Methods

        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        public object GetService(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : null;
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveAll(serviceType) as IEnumerable<object>;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _scope.Dispose();
        }

        #endregion
    }
}
