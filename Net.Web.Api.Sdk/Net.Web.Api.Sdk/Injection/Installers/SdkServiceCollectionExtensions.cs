using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Attributes;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <summary>
    /// Extension methods for registering SDK services via assembly scanning.
    /// Replaces the Castle Windsor ServiceInstaller.
    /// </summary>
    public static class SdkServiceCollectionExtensions
    {
        /// <summary>
        /// Scans loaded assemblies matching the given prefix for interfaces marked with
        /// <see cref="InjectInterfaceServiceAttribute"/> and registers their implementing
        /// classes as singletons. When multiple implementations exist for the same interface,
        /// the one decorated with <see cref="InjectServiceCustomAttribute"/> takes precedence.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">The assembly name prefix to filter by (optional).</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSdkServices(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            var registrationList = GetRegistrationList(assemblyNamePrefix);

            foreach (var item in registrationList)
            {
                services.AddSingleton(item.Key, item.Value);
            }

            return services;
        }

        /// <summary>
        /// Gets the interfaces marked with <see cref="InjectInterfaceServiceAttribute"/> that the class implements.
        /// </summary>
        /// <param name="class">The class type.</param>
        /// <returns>List of marked interfaces.</returns>
        private static IList<Type> GetInterfaces(Type @class)
        {
            return @class.GetInterfaces()
                .Where(@interface => @interface.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null)
                .ToList();
        }

        /// <summary>
        /// Builds the registration dictionary mapping interfaces to their implementation types.
        /// </summary>
        /// <param name="assemblyNamePrefix">The assembly name prefix to filter by.</param>
        /// <returns>Dictionary mapping interface types to implementation types.</returns>
        private static Dictionary<Type, Type> GetRegistrationList(string assemblyNamePrefix)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName != null && !a.IsDynamic)
                .ToList();

            var classes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                classes.AddRange(
                    types.Where(
                        @object => @object.IsClass &&
                        !@object.IsAbstract &&
                        (
                            string.IsNullOrEmpty(assemblyNamePrefix) ||
                            @object.Assembly.FullName!.StartsWith(assemblyNamePrefix,
                            StringComparison.CurrentCultureIgnoreCase)
                        )
                    )
                );
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(assemblyNamePrefix) && !@class.Assembly.FullName!.StartsWith(assemblyNamePrefix,
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

                    if (@interface1.GetInterfaces().FirstOrDefault(c => c.FullName!.Equals(@interface2.FullName)) != null)
                    {
                        @interface = @interface1;
                    }
                    else if (@interface2.GetInterfaces().FirstOrDefault(c => c.FullName!.Equals(@interface1.FullName)) != null)
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
        /// Determines whether the specified class is decorated with <see cref="InjectServiceCustomAttribute"/>.
        /// </summary>
        /// <param name="class">The class type.</param>
        /// <returns><c>true</c> if the class has the custom service attribute; otherwise, <c>false</c>.</returns>
        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).FirstOrDefault() != null;
        }
    }
}
