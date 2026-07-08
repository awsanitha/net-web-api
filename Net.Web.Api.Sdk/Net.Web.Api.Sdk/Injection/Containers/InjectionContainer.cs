using System;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class InjectionContainer. This class cannot be inherited.
    /// Provides access to the service provider for scenarios where constructor injection is not available.
    /// </summary>
    public sealed class InjectionContainer
    {
        #region Singleton

        private static readonly Lazy<InjectionContainer> _lazy = new Lazy<InjectionContainer>(() => new InjectionContainer());

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static InjectionContainer Instance => _lazy.Value;

        #endregion

        #region Private Properties

        private IServiceProvider? _serviceProvider;

        #endregion

        #region Constructors

        private InjectionContainer() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>T.</returns>
        public T GetService<T>() where T : notnull
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider has not been initialized. Call SetServiceProvider first.");
            }

            return _serviceProvider.GetRequiredService<T>();
        }

        #endregion
    }
}
