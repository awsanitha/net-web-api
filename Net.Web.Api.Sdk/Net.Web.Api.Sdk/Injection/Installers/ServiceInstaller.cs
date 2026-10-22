using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Attributes;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <summary>
    /// Class ServiceInstaller.
    /// Scans application assemblies and registers services decorated with
    /// <see cref="InjectInterfaceServiceAttribute"/> into the ASP.NET Core DI container.
    /// </summary>
    public class ServiceInstaller
    {
        #region Private Properties

        /// <summary>
        /// The assembly name prefix
        /// </summary>
        private readonly string _assemblyNamePrefix;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInstaller"/> class.
        /// </summary>
        /// <param name="assemblyNamePrefix">The assembly name prefix.</param>
        public ServiceInstaller(string assemblyNamePrefix = null)
        {
            _assemblyNamePrefix = assemblyNamePrefix;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers services found in assemblies into the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void Install(IServiceCollection services)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrEmpty(baseDirectory))
            {
                return;
            }

            var assemblies = GetAssemblies(baseDirectory);
            var registrationList = GetRegistrationList(assemblies);

            foreach (var item in registrationList)
            {
                if (!services.Any(d => d.ServiceType == item.Key))
                {
                    services.AddSingleton(item.Key, item.Value);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the interfaces decorated with <see cref="InjectInterfaceServiceAttribute"/>.
        /// </summary>
        private static IList<Type> GetInterfaces(Type @class)
        {
            return @class.GetInterfaces()
                .Where(@interface => @interface.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null)
                .ToList();
        }

        /// <summary>
        /// Gets the registration list.
        /// </summary>
        private Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies)
        {
            var classes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    classes.AddRange(
                        assembly.GetTypes()
                            .Where(
                                @object => @object.IsClass &&
                                !@object.IsAbstract &&
                                (
                                    string.IsNullOrEmpty(_assemblyNamePrefix) ||
                                    @object.Assembly.FullName.StartsWith(_assemblyNamePrefix,
                                    StringComparison.CurrentCultureIgnoreCase)
                                )
                            )
                    );
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that cannot be reflected
                }
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(_assemblyNamePrefix) &&
                    !@class.Assembly.FullName.StartsWith(_assemblyNamePrefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var interfaces = GetInterfaces(@class);

                if (interfaces.Count == 0 || interfaces.Count > 2)
                {
                    continue;
                }

                Type @interface = null;

                if (interfaces.Count == 2)
                {
                    var interface1 = interfaces[0];
                    var interface2 = interfaces[1];

                    if (interface1.GetInterfaces().Any(c => c.FullName == interface2.FullName))
                    {
                        @interface = interface1;
                    }
                    else if (interface2.GetInterfaces().Any(c => c.FullName == interface1.FullName))
                    {
                        @interface = interface2;
                    }
                }
                else
                {
                    @interface = interfaces[0];
                }

                if (@interface == null)
                {
                    continue;
                }

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

                var existingClass = result[item.Key];
                var currentClass = item.Value;
                var isExistingClassCustom = IsCustomService(existingClass);
                var isCurrentClassCustom = IsCustomService(currentClass);

                if (!isExistingClassCustom && isCurrentClassCustom)
                {
                    result[item.Key] = currentClass;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified class is a custom service.
        /// </summary>
        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).Any();
        }

        /// <summary>
        /// Gets the list of assemblies from the base directory.
        /// </summary>
        private static IEnumerable<Assembly> GetAssemblies(string baseDirectory)
        {
            var result = new List<Assembly>();
            var files = System.IO.Directory.GetFiles(baseDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    result.Add(assembly);
                }
                catch
                {
                    // Skip assemblies that cannot be loaded
                }
            }

            return result;
        }

        #endregion
    }
}
