namespace Net.Web.Api.Sdk.Common.Constants
{
    /// <summary>
    /// Routing constants shared across SDK controllers.
    /// </summary>
    public static class RouteConstants
    {
        /// <summary>
        /// Route template for versioned endpoints: <c>api/v{version:apiVersion}</c>.
        /// </summary>
        public const string ROUTE_PREFIX_VERSION = "api/v{version:apiVersion}";

        /// <summary>
        /// The URL segment parameter name used by Asp.Versioning.
        /// </summary>
        internal const string API_VERSION_FIELD = "apiVersion";
    }
}
