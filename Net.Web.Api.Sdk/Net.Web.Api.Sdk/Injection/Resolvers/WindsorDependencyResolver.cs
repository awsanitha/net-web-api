using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Resolvers
{
    /// <summary>
    /// Class WindsorDependencyResolver - bridges Castle Windsor with ASP.NET Core DI.
    /// </summary>
    public class WindsorDependencyResolver : IServiceProvider, ISupportRequiredService
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

        #region IServiceProvider Implementations

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType) ? _container.Resolve(serviceType) : null;
        }

        /// <inheritdoc />
        public object GetRequiredService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        #endregion
    }
}
