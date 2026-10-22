using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Attributes;

namespace Net.Web.Api.Sdk.Injection.Installers
{
    /// <summary>
    /// Class ServiceInstaller. Discovers and registers services using IServiceCollection.
    /// </summary>
    public class ServiceInstaller
    {
        #region Private Properties

        private readonly string _assemblyNamePrefix;

        #endregion

        #region Constructors

        public ServiceInstaller(string assemblyNamePrefix = null)
        {
            _assemblyNamePrefix = assemblyNamePrefix;
        }

        #endregion

        #region Public Methods

        public void Install(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
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

        private static IList<Type> GetInterfaces(Type @class)
        {
            return @class.GetInterfaces()
                .Where(@interface => @interface.GetCustomAttribute(typeof(InjectInterfaceServiceAttribute), false) != null)
                .ToList();
        }

        private Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies)
        {
            var classes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                classes.AddRange(
                    assembly.GetTypes()
                        .Where(
                            @object => @object.IsClass &&
                            !@object.IsAbstract &&
                            (
                                string.IsNullOrEmpty(_assemblyNamePrefix) ||
                                @object.Assembly.FullName.StartsWith(_assemblyNamePrefix, StringComparison.CurrentCultureIgnoreCase)
                            )
                        ).ToList()
                );
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(_assemblyNamePrefix) && !@class.Assembly.FullName.StartsWith(_assemblyNamePrefix, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                var interfaces = GetInterfaces(@class);

                if (interfaces.Count == 0 || interfaces.Count > 2)
                    continue;

                Type @interface = null;

                if (interfaces.Count == 2)
                {
                    var @interface1 = interfaces[0];
                    var @interface2 = interfaces[1];

                    if (@interface1.GetInterfaces().FirstOrDefault(c => c.FullName.Equals(@interface2.FullName)) != null)
                        @interface = @interface1;
                    else if (@interface2.GetInterfaces().FirstOrDefault(c => c.FullName.Equals(@interface1.FullName)) != null)
                        @interface = @interface2;
                }
                else
                {
                    @interface = interfaces[0];
                }

                if (@interface == null)
                    continue;

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
                    result[item.Key] = currentClass;
            }

            return result;
        }

        private static bool IsCustomService(Type @class)
        {
            return @class.GetCustomAttributes(typeof(InjectServiceCustomAttribute), false).FirstOrDefault() != null;
        }

        #endregion
    }
}
