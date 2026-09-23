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
    /// Scans assemblies for interfaces marked with <see cref="InjectInterfaceServiceAttribute"/> and
    /// registers the best matching implementation into the ASP.NET Core dependency injection container.
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
        /// Installs the discovered services into the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        public void Install(IServiceCollection services)
        {
            var assemblies = GetCandidateAssemblies();
            var registrationList = GetRegistrationList(assemblies);

            foreach (var item in registrationList)
            {
                services.AddSingleton(item.Key, item.Value);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the candidate assemblies to scan.
        /// </summary>
        /// <returns>IEnumerable&lt;Assembly&gt;.</returns>
        private static IEnumerable<Assembly> GetCandidateAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
        }

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
        /// <returns>Dictionary&lt;Type, Type&gt;.</returns>
        private Dictionary<Type, Type> GetRegistrationList(IEnumerable<Assembly> assemblies)
        {
            var classes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                classes.AddRange(
                    types
                        .Where(
                            @object => @object.IsClass &&
                            !@object.IsAbstract &&
                            (
                                string.IsNullOrEmpty(_assemblyNamePrefix) ||
                                @object.Assembly.FullName.StartsWith(_assemblyNamePrefix,
                                StringComparison.CurrentCultureIgnoreCase)
                            )
                        ).ToList()
                );
            }

            var registrationList = new List<KeyValuePair<Type, Type>>();

            foreach (var @class in classes)
            {
                if (!string.IsNullOrEmpty(_assemblyNamePrefix) && !@class.Assembly.FullName.StartsWith(_assemblyNamePrefix,
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
