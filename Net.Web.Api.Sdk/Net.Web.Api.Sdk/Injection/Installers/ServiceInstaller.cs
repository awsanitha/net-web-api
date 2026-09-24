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
    /// Provides extension methods for registering services via assembly scanning.
    /// </summary>
    public static class ServiceInstaller
    {
        #region Public Methods

        /// <summary>
        /// Registers all services marked with [InjectInterfaceService] from loaded assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">Optional assembly name prefix filter.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddSdkServices(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                .ToList();

            var registrationList = GetRegistrationList(assemblies, assemblyNamePrefix);

            foreach (var item in registrationList)
            {
                services.AddSingleton(item.Key, item.Value);
            }

            return services;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the interfaces.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <returns>IList&lt;Type&gt;.</returns>
        private static IList<Type> GetInterfaces(Type @class)
        {
            return @class.GetInterfaces().Where(@interface => @interface.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null).ToList();
        }

        /// <summary>
        /// Gets the registration list.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="assemblyNamePrefix">The assembly name prefix.</param>
        /// <returns>Dictionary&lt;Type, Type&gt;.</returns>
        private static Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies, string assemblyNamePrefix)
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
                                    string.IsNullOrEmpty(assemblyNamePrefix) ||
                                    @object.Assembly.FullName.StartsWith(assemblyNamePrefix,
                                    StringComparison.CurrentCultureIgnoreCase)
                                )
                            ).ToList()
                    );
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(assemblyNamePrefix) && !@class.Assembly.FullName.StartsWith(assemblyNamePrefix,
                    StringComparison.CurrentCultureIgnoreCase))
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
                    var @interface1 = interfaces[0];
                    var @interface2 = interfaces[1];

                    if (@interface1.GetInterfaces().FirstOrDefault(c => c.FullName.Equals(@interface2.FullName)) != null)
                    {
                        @interface = @interface1;
                    }
                    else if (@interface2.GetInterfaces().FirstOrDefault(c => c.FullName.Equals(@interface1.FullName)) != null)
                    {
                        @interface = @interface2;
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

                var @existingClass = result[item.Key];
                var @currentClass = item.Value;
                var isExistingClassCustom = IsCustomService(@existingClass);
                var isCurrentClassCustom = IsCustomService(@currentClass);

                if (!isExistingClassCustom && isCurrentClassCustom)
                {
                    result[item.Key] = @currentClass;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether [is custom service] [the specified class].
        /// </summary>
        /// <param name="class">The class.</param>
        /// <returns><c>true</c> if [is custom service] [the specified class]; otherwise, <c>false</c>.</returns>
        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).FirstOrDefault() != null;
        }

        #endregion
    }
}
