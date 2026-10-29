using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;

namespace Net.Web.Api.Sdk.Injection.Filters
{
    /// <summary>
    /// Class SdkAssemblyFilter.
    /// Builds the assembly filter used to discover controllers and services. On .NET Framework the
    /// scan used <c>AppDomain.CurrentDomain.RelativeSearchPath</c>, which pointed at the web
    /// application <c>bin</c> folder and therefore held application assemblies only. On modern .NET
    /// the application directory also holds the framework and package assemblies, so they are
    /// excluded explicitly to keep the scan both fast and safe.
    /// </summary>
    internal static class SdkAssemblyFilter
    {
        #region Private Properties

        /// <summary>
        /// The assembly name prefixes that never contain SDK controllers or services
        /// </summary>
        private static readonly string[] _excludedPrefixes =
        {
            "System.",
            "Microsoft.",
            "mscorlib",
            "netstandard",
            "WindowsBase",
            "Castle.",
            "Newtonsoft.",
            "NuGet.",
            "Swashbuckle.",
            "Asp.Versioning",
            "LiteDB",
            "ByteSize"
        };

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates the assembly filter for the application directory.
        /// </summary>
        /// <returns>AssemblyFilter.</returns>
        internal static AssemblyFilter Create()
        {
            return new AssemblyFilter(AppContext.BaseDirectory).FilterByName(IsApplicationAssembly);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the assembly is an application assembly worth scanning.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns><c>true</c> when the assembly must be scanned; otherwise, <c>false</c>.</returns>
        private static bool IsApplicationAssembly(AssemblyName assemblyName)
        {
            var name = assemblyName?.Name;

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return !_excludedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
