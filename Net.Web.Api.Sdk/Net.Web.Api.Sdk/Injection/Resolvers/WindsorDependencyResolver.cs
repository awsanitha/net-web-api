using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Scopes;

namespace Net.Web.Api.Sdk.Injection.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Class WindsorDependencyResolver.
    /// Resolves services out of the Windsor container for ASP.NET Core, as the Web API
    /// <c>IDependencyResolver</c> implementation did.
    /// </summary>
    /// <seealso cref="T:System.IServiceProvider" />
    public class WindsorDependencyResolver : IServiceProvider, IServiceScopeFactory, IDisposable
    {
        #region Private Properties

        /// <summary>
        /// The container
        /// </summary>
        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorDependencyResolver(IWindsorContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        #endregion

        #region IServiceScopeFactory Implementations

        /// <inheritdoc />
        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        /// <returns>The dependency scope.</returns>
        public IServiceScope CreateScope()
        {
            return new WindsorDependencyScope(_container);
        }

        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        /// <returns>The dependency scope.</returns>
        public WindsorDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(_container);
        }

        #endregion

        #region IServiceProvider Implementations

        /// <inheritdoc />
        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service.</returns>
        public object GetService(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : null;
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>The retrieved collection of services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return !_container.Kernel.HasComponent(serviceType)
                ? Array.Empty<object>()
                : _container.ResolveAll(serviceType).Cast<object>();
        }

        #endregion

        #region IDisposable Implementations

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _container.Dispose();
        }

        #endregion
    }
}
