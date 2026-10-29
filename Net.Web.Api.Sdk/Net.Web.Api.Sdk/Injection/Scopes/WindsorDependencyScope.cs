using System;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Scopes
{
    /// <inheritdoc />
    /// <summary>
    /// Class WindsorDependencyScope.
    /// Wraps a Windsor lifetime scope as an ASP.NET Core service scope.
    /// </summary>
    /// <seealso cref="T:Microsoft.Extensions.DependencyInjection.IServiceScope" />
    public class WindsorDependencyScope : IServiceScope, IServiceProvider
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyScope"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorDependencyScope(IWindsorContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _scope = container.BeginScope();
        }

        #endregion

        #region IServiceScope Implementations

        /// <inheritdoc />
        /// <summary>
        /// Gets the service provider of the scope.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider => this;

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

        #endregion

        #region IDisposable Implementations

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _scope.Dispose();
        }

        #endregion
    }
}
