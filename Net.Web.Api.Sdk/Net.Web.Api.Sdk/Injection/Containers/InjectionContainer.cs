using System;
using Castle.Windsor;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Singleton accessor for the Castle.Windsor container used by the SDK.
    /// </summary>
    public sealed class InjectionContainer
    {
        #region Singleton

        private static readonly Lazy<InjectionContainer> _lazy =
            new Lazy<InjectionContainer>(() => new InjectionContainer());

        /// <summary>Gets the singleton instance.</summary>
        public static InjectionContainer Instance => _lazy.Value;

        #endregion

        #region Private Fields

        private IWindsorContainer _container;
        private string _contentRootPath;

        #endregion

        #region Constructors

        private InjectionContainer() { }

        #endregion

        #region Public Methods

        /// <summary>Stores the Windsor container for later use.</summary>
        public void SetContainer(IWindsorContainer container)
        {
            _container = container;
        }

        /// <summary>Stores the application content-root path used by services that need file I/O.</summary>
        public void SetContentRootPath(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        /// <summary>Gets the application content-root path.</summary>
        public string GetContentRootPath() => _contentRootPath;

        /// <summary>Resolves a service from the Windsor container.</summary>
        public T GetService<T>()
        {
            return _container != null && _container.Kernel.HasComponent(typeof(T))
                ? _container.Resolve<T>()
                : default;
        }

        /// <summary>
        /// Resolves a service, passing the content-root path to services that require it
        /// (e.g. <see cref="Interfaces.Token.IJwtTokenService"/>).
        /// </summary>
        public T GetService<T>(string contentRootPath)
        {
            if (!string.IsNullOrEmpty(contentRootPath))
            {
                _contentRootPath = contentRootPath;
            }

            return GetService<T>();
        }

        #endregion

        #region Internal Methods

        internal void DisposeContainer()
        {
            _container?.Dispose();
        }

        #endregion
    }
}
