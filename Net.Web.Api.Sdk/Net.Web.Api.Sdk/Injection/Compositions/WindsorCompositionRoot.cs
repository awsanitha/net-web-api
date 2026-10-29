using System;
using Castle.Windsor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Net.Web.Api.Sdk.Injection.Compositions
{
    /// <inheritdoc />
    /// <summary>
    /// Class WindsorCompositionRoot.
    /// Activates MVC controllers from the Windsor container, as the Web API
    /// <c>IHttpControllerActivator</c> implementation did.
    /// </summary>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.Controllers.IControllerActivator" />
    public class WindsorCompositionRoot : IControllerActivator
    {
        #region Private Properties

        /// <summary>
        /// The container
        /// </summary>
        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorCompositionRoot"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorCompositionRoot(IWindsorContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        #endregion

        #region IControllerActivator Implementations

        /// <inheritdoc />
        /// <summary>
        /// Creates a controller instance for the current request.
        /// </summary>
        /// <param name="context">The controller context.</param>
        /// <returns>The controller instance.</returns>
        public object Create(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _container.Resolve(context.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        /// <inheritdoc />
        /// <summary>
        /// Releases a controller instance at the end of the request.
        /// </summary>
        /// <param name="context">The controller context.</param>
        /// <param name="controller">The controller.</param>
        public void Release(ControllerContext context, object controller)
        {
            if (controller == null)
            {
                return;
            }

            _container.Release(controller);
        }

        #endregion
    }
}
