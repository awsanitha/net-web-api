using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Net.Web.Api.Sdk.Injection.Attributes;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <summary>
    /// Castle.Windsor installer that scans all loaded assemblies and registers any class whose
    /// interface is decorated with <see cref="InjectInterfaceServiceAttribute"/>.
    /// </summary>
    public class ServiceInstaller : IWindsorInstaller
    {
        #region Private Fields

        private readonly string _assemblyNamePrefix;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceInstaller"/>.
        /// </summary>
        /// <param name="assemblyNamePrefix">
        /// Optional assembly-name prefix filter. When supplied, only classes whose assembly name
        /// starts with this value are considered for registration.
        /// </param>
        public ServiceInstaller(string assemblyNamePrefix = null)
        {
            _assemblyNamePrefix = assemblyNamePrefix;
        }

        #endregion

        #region IWindsorInstaller

        /// <inheritdoc />
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Use assemblies already loaded in the current AppDomain (works reliably on .NET 10).
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .ToList();

            var registrationList = GetRegistrationList(assemblies);

            foreach (var item in registrationList)
            {
                if (container.Kernel.HasComponent(item.Key))
                    continue;

                container.Register(
                    Component.For(item.Key, item.Value)
                        .ImplementedBy(item.Value)
                        .Named(item.Key.FullName)
                        .LifestyleSingleton());
            }
        }

        #endregion

        #region Private Methods

        private static IList<Type> GetInterfaces(Type @class)
        {
            return @class.GetInterfaces()
                .Where(i => i.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null)
                .ToList();
        }

        private Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies)
        {
            var classes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    classes.AddRange(
                        assembly.GetTypes()
                            .Where(t => t.IsClass && !t.IsAbstract &&
                                (string.IsNullOrEmpty(_assemblyNamePrefix) ||
                                 t.Assembly.FullName?.StartsWith(_assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) == true)));
                }
                catch
                {
                    // Skip assemblies that cannot be reflected
                }
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(_assemblyNamePrefix) &&
                    @class.Assembly.FullName?.StartsWith(_assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                var interfaces = GetInterfaces(@class);

                if (interfaces.Count == 0 || interfaces.Count > 2)
                    continue;

                Type @interface = null;

                if (interfaces.Count == 2)
                {
                    var i1 = interfaces[0];
                    var i2 = interfaces[1];

                    if (i1.GetInterfaces().Any(c => c.FullName == i2.FullName))
                        @interface = i1;
                    else if (i2.GetInterfaces().Any(c => c.FullName == i1.FullName))
                        @interface = i2;
                }
                else
                {
                    @interface = interfaces[0];
                }

                if (@interface == null) continue;

                registrationList.Add(new KeyValuePair<Type, Type>(@interface, @class));
            }

            registrationList = registrationList.OrderBy(c => c.Key.FullName).ToList();

            var result = new Dictionary<Type, Type>();

            foreach (var item in registrationList)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                    continue;
                }

                var existing = result[item.Key];
                var current = item.Value;

                if (!IsCustomService(existing) && IsCustomService(current))
                    result[item.Key] = current;
            }

            return result;
        }

        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).Any();
        }

        #endregion
    }
}
