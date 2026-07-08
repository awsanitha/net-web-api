using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Attributes;

namespace Net.Web.Api.Sdk.Injection.Resolvers
{
    /// <summary>
    /// Class ServiceRegistrar. Registers services with [InjectInterfaceService] attribute from loaded assemblies.
    /// Replaces the Castle Windsor assembly scanning with built-in DI registration.
    /// </summary>
    public static class ServiceRegistrar
    {
        /// <summary>
        /// Registers all services marked with <see cref="InjectInterfaceServiceAttribute"/> in the loaded assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblyNamePrefix">Optional prefix to filter assemblies by name.</param>
        public static IServiceCollection RegisterSdkServices(this IServiceCollection services, string? assemblyNamePrefix = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic &&
                            (string.IsNullOrEmpty(assemblyNamePrefix) ||
                             a.FullName?.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            // Also try to load assemblies from the base directory
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDirectory) && !string.IsNullOrEmpty(assemblyNamePrefix))
            {
                var assemblyFiles = System.IO.Directory.GetFiles(baseDirectory, $"{assemblyNamePrefix}*.dll");
                foreach (var file in assemblyFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        if (!assemblies.Contains(assembly))
                        {
                            assemblies.Add(assembly);
                        }
                    }
                    catch
                    {
                        // Ignore assembly load errors
                    }
                }
            }

            var registrationMap = new Dictionary<Type, Type>();
            var priorityMap = new Dictionary<Type, bool>(); // true = custom

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> types;

                try
                {
                    types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract &&
                                    (string.IsNullOrEmpty(assemblyNamePrefix) ||
                                     t.Assembly.FullName?.StartsWith(assemblyNamePrefix, StringComparison.OrdinalIgnoreCase) == true));
                }
                catch
                {
                    continue;
                }

                foreach (var classType in types)
                {
                    var interfaces = classType.GetInterfaces()
                        .Where(i => i.GetCustomAttribute<InjectInterfaceServiceAttribute>() != null)
                        .ToList();

                    if (interfaces.Count == 0 || interfaces.Count > 2)
                    {
                        continue;
                    }

                    Type? resolvedInterface = null;

                    if (interfaces.Count == 2)
                    {
                        var i1 = interfaces[0];
                        var i2 = interfaces[1];

                        if (i1.GetInterfaces().Any(c => c.FullName == i2.FullName))
                        {
                            resolvedInterface = i1;
                        }
                        else if (i2.GetInterfaces().Any(c => c.FullName == i1.FullName))
                        {
                            resolvedInterface = i2;
                        }
                    }
                    else
                    {
                        resolvedInterface = interfaces[0];
                    }

                    if (resolvedInterface == null)
                    {
                        continue;
                    }

                    var isCustom = classType.GetCustomAttribute<InjectServiceCustomAttribute>() != null;

                    if (!registrationMap.ContainsKey(resolvedInterface))
                    {
                        registrationMap[resolvedInterface] = classType;
                        priorityMap[resolvedInterface] = isCustom;
                    }
                    else if (!priorityMap[resolvedInterface] && isCustom)
                    {
                        // Custom service overrides default
                        registrationMap[resolvedInterface] = classType;
                        priorityMap[resolvedInterface] = true;
                    }
                }
            }

            foreach (var kvp in registrationMap.OrderBy(r => r.Key.FullName))
            {
                services.AddSingleton(kvp.Key, kvp.Value);
            }

            return services;
        }
    }
}
