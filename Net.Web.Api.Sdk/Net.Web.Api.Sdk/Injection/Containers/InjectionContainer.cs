using System;
using Microsoft.Extensions.DependencyInjection;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class InjectionContainer. This class cannot be inherited.
    /// </summary>
    public sealed class InjectionContainer
    {
        #region Singleton

        private static readonly Lazy<InjectionContainer> _lazy = new Lazy<InjectionContainer>(() => new InjectionContainer());

        public static InjectionContainer Instance => _lazy.Value;

        #endregion

        #region Private Properties

        private IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        private InjectionContainer() { }

        #endregion

        #region Public Methods

        public void SetContainer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        #endregion

        #region Internal Methods

        internal void DisposeContainer() { }

        #endregion
    }
}
