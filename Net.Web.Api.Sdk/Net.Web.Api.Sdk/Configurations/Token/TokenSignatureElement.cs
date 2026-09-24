namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenSignatureElement.
    /// POCO replacement for the former ConfigurationElement.
    /// </summary>
    public class TokenSignatureElement
    {
        /// <summary>
        /// Gets or sets the pass phrase.
        /// </summary>
        /// <value>The pass phrase.</value>
        public string PassPhrase { get; set; } = "";

        /// <summary>
        /// Gets or sets the signing certificate.
        /// </summary>
        /// <value>The signing certificate.</value>
        public string SigningCertificate { get; set; } = "";

        /// <summary>
        /// Gets or sets the signing certificate password.
        /// </summary>
        /// <value>The signing certificate password.</value>
        public string SigningCertificatePassword { get; set; } = "";

        /// <summary>
        /// Gets or sets the validating certificate.
        /// </summary>
        /// <value>The validating certificate.</value>
        public string ValidatingCertificate { get; set; } = "";
    }
}
