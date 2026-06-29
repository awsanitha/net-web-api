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
    /// </summary>
    public static class ServiceInstaller
    {
        /// <summary>
        /// Scans loaded assemblies and registers services whose interfaces have
        /// <see cref="InjectInterfaceServiceAttribute"/> as singletons.
        /// </summary>
        public static IServiceCollection InstallServices(this IServiceCollection services, string assemblyNamePrefix = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic &&
                            (string.IsNullOrEmpty(assemblyNamePrefix) ||
                             a.FullName.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var registrations = GetRegistrationList(assemblies);

            foreach (var item in registrations)
            {
                services.AddSingleton(item.Key, item.Value);
            }

            return services;
        }

        private static Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies)
        {
            var classes = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                var interfaces = GetInjectableInterfaces(@class);

                if (interfaces.Count == 0 || interfaces.Count > 2) continue;

                Type iface = null;

                if (interfaces.Count == 2)
                {
                    var i1 = interfaces[0];
                    var i2 = interfaces[1];
                    if (i1.GetInterfaces().Any(c => c.FullName == i2.FullName))
                        iface = i1;
                    else if (i2.GetInterfaces().Any(c => c.FullName == i1.FullName))
                        iface = i2;
                }
                else
                {
                    iface = interfaces[0];
                }

                if (iface == null) continue;

                registrationList.Add(new KeyValuePair<Type, Type>(iface, @class));
            }

            registrationList = registrationList.OrderBy(c => c.Key.FullName).ToList();

            var result = new Dictionary<Type, Type>();

            foreach (var item in registrationList)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                }
                else
                {
                    var existingIsCustom = IsCustomService(result[item.Key]);
                    var currentIsCustom = IsCustomService(item.Value);
                    if (!existingIsCustom && currentIsCustom)
                        result[item.Key] = item.Value;
                }
            }

            return result;
        }

        private static IList<Type> GetInjectableInterfaces(Type @class)
        {
            return @class.GetInterfaces()
                .Where(i => i.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null)
                .ToList();
        }

        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).Any();
        }
    }
}
