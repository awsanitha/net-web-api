using System;
using System.Collections.Generic;
using Castle.Windsor;
using Net.Web.Api.Sdk.Injection.Scopes;

namespace Net.Web.Api.Sdk.Injection.Resolvers
{
    /// <summary>
    /// Class WindsorDependencyResolver.
    /// </summary>
    public class WindsorDependencyResolver : IDisposable
    {
        #region Private Properties

        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyResolver"/> class.
        /// </summary>
        public WindsorDependencyResolver(IWindsorContainer container)
        {
            _container = container;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        public WindsorDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(_container);
        }

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
            return !_container.Kernel.HasComponent(serviceType) ? new object[0] : _container.ResolveAll(serviceType) as IEnumerable<object>;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _container.Dispose();
        }

        #endregion
    }
}
