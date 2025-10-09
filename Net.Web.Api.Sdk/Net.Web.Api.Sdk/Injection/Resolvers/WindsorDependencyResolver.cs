using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Resolvers
{
    /// <summary>
    /// Class WindsorDependencyResolver.
    /// Provides service resolution using Castle Windsor for ASP.NET Core
    /// </summary>
    public class WindsorDependencyResolver : IServiceProvider
    {
        #region Private Properties

        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorDependencyResolver(IWindsorContainer container)
        {
            _container = container;
        }

        #endregion

        #region IServiceProvider Implementations

        /// <inheritdoc />
        /// <summary>
        /// Retrieves a service from the container.
        /// </summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service.</returns>
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves a collection of services from the container.
        /// </summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>The retrieved collection of services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return !_container.Kernel.HasComponent(serviceType) ? new object[0] : _container.ResolveAll(serviceType).Cast<object>();
            }
            catch
            {
                return new object[0];
            }
        }

        /// <summary>
        /// Resolves a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The resolved service.</returns>
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
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
    }
}
