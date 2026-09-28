using System;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class InjectionContainer. This class cannot be inherited.
    /// Provides a static service locator for contexts where constructor injection
    /// is not available (e.g., validation attributes, authorization attributes).
    /// </summary>
    public sealed class InjectionContainer
    {
        #region Singleton

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static readonly Lazy<InjectionContainer> _lazy = new Lazy<InjectionContainer>(() => new InjectionContainer());

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>The instance.</value>
        public static InjectionContainer Instance => _lazy.Value;

        #endregion

        #region Private Properties

        /// <summary>
        /// The service provider.
        /// </summary>
        private IServiceProvider _provider;

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
        /// <param name="provider">The service provider.</param>
        public void SetContainer(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the container has not been initialized.</exception>
        public T GetService<T>() where T : class
        {
            if (_provider == null)
                throw new InvalidOperationException("InjectionContainer has not been initialized. Call SetContainer first.");
            return _provider.GetRequiredService<T>();
        }

        #endregion
    }
}
