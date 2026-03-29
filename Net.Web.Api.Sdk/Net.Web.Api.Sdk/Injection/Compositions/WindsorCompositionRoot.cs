using System;
using Castle.Windsor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Net.Web.Api.Sdk.Injection.Compositions
{
    /// <summary>
    /// Class WindsorCompositionRoot - IControllerActivator backed by Castle Windsor.
    /// </summary>
    public class WindsorCompositionRoot : IControllerActivator
    {
        #region Private Properties

        private readonly IWindsorContainer _container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorCompositionRoot"/> class.
        /// </summary>
        public WindsorCompositionRoot(IWindsorContainer container)
        {
            _container = container;
        }

        #endregion

        #region IControllerActivator Implementations

        /// <inheritdoc />
        public object Create(ControllerContext context)
        {
            return _container.Resolve(context.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        /// <inheritdoc />
        public void Release(ControllerContext context, object controller)
        {
            _container.Release(controller);
        }

        #endregion
    }
}
