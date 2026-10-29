namespace Net.Web.Api.Sdk.Common.Constants
{
    /// <summary>
    /// Class SdkCorsConstants.
    /// </summary>
    public static class SdkCorsConstants
    {
        /// <summary>
        /// The name of the permissive CORS policy registered by the SDK. It replaces the
        /// legacy <c>[EnableCors("*", "*", "*")]</c> attribute arguments, which ASP.NET Core
        /// expresses as a named policy instead.
        /// </summary>
        public const string DEFAULT_POLICY_NAME = "NetWebApiSdkDefaultCorsPolicy";
    }
}
