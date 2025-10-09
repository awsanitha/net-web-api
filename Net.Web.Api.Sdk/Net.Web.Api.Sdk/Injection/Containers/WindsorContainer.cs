using System;
using Castle.Windsor;

namespace Net.Web.Api.Sdk.Injection.Containers
{
    /// <summary>
    /// Class WindsorContainer. This class cannot be inherited.
    /// </summary>
    public sealed class WindsorContainer
    {
        #region Singleton

        /// <summary>
        /// The lazy
        /// </summary>
        private static readonly Lazy<IWindsorContainer> _lazy = new Lazy<IWindsorContainer>(() => new Castle.Windsor.WindsorContainer());

        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IWindsorContainer Instance { get; set; } = _lazy.Value;

        #endregion
    }
}
