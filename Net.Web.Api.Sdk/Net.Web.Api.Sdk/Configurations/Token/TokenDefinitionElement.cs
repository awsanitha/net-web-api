namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenDefinitionElement.
    /// POCO replacement for the former ConfigurationElement.
    /// </summary>
    public class TokenDefinitionElement
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>The issuer.</value>
        public string Issuer { get; set; } = "http://no.where.com";

        /// <summary>
        /// Gets or sets the intended audience.
        /// </summary>
        /// <value>The intended audience.</value>
        public string IntendedAudience { get; set; } = "urn:no-where";

        /// <summary>
        /// Gets or sets the expiration in minute.
        /// </summary>
        /// <value>The expiration in minute.</value>
        public double ExpirationInMinute { get; set; } = 15d;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is base64 encoded.
        /// </summary>
        /// <value><c>true</c> if this instance is base64 encoded; otherwise, <c>false</c>.</value>
        public bool IsBase64Encoded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [one time use].
        /// </summary>
        /// <value><c>true</c> if [one time use]; otherwise, <c>false</c>.</value>
        public bool OneTimeUse { get; set; }

        /// <summary>
        /// Gets the signature.
        /// </summary>
        /// <value>The signature.</value>
        public TokenSignatureElement Signature { get; set; }
    }
}
