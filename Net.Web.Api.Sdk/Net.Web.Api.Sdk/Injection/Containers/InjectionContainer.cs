using System;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class InjectionContainer. This class cannot be inherited.
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
        /// The service provider backing the container.
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
        /// Sets the underlying <see cref="IServiceProvider"/> used to resolve services.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        public void SetContainer(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T GetService<T>()
        {
            return (T)_provider.GetService(typeof(T));
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Disposes the container.
        /// </summary>
        internal void DisposeContainer()
        {
            (_provider as IDisposable)?.Dispose();
        }

        #endregion

    }
}
