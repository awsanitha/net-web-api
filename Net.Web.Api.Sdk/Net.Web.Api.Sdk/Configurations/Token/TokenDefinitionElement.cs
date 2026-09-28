namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenDefinitionOptions.
    /// Options POCO replacing the former ConfigurationElement.
    /// </summary>
    public class TokenDefinitionOptions
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>The issuer.</value>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the intended audience.
        /// </summary>
        /// <value>The intended audience.</value>
        public string IntendedAudience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiration in minute.
        /// </summary>
        /// <value>The expiration in minute.</value>
        public double ExpirationInMinute { get; set; }

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
        /// Gets or sets the signature.
        /// </summary>
        /// <value>The signature.</value>
        public TokenSignatureOptions Signature { get; set; } = new();
    }
}
