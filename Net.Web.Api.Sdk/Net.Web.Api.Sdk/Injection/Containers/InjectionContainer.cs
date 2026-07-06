using System;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class InjectionContainer. This class cannot be inherited.
    /// Provides a static service locator for services that cannot use constructor injection.
    /// </summary>
    public sealed class InjectionContainer
    {
        #region Singleton

        /// <summary>
        /// The lazy
        /// </summary>
        private static readonly Lazy<InjectionContainer> _lazy = new Lazy<InjectionContainer>(() => new InjectionContainer());

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static InjectionContainer Instance => _lazy.Value;

        #endregion

        #region Private Properties

        /// <summary>
        /// The service provider
        /// </summary>
        private IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="InjectionContainer"/> class from being created.
        /// </summary>
        private InjectionContainer() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public void SetContainer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Disposes the container.
        /// </summary>
        internal void DisposeContainer()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        #endregion
    }
}
