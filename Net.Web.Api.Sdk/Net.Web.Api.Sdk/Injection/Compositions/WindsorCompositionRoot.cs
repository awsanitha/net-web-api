using Castle.Windsor;

namespace Net.Web.Api.Sdk.Injection.Compositions
{
    /// <summary>
    /// Class WindsorCompositionRoot. Retained for backward compatibility.
    /// In ASP.NET Core, use the built-in DI or a Windsor integration package.
    /// </summary>
    public class WindsorCompositionRoot
    {
        private readonly IWindsorContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorCompositionRoot"/> class.
        /// </summary>
        public WindsorCompositionRoot(IWindsorContainer container)
        {
            _container = container;
        }
    }
}
