using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenSecurityElement.
    /// Plain object representation of a <c>signature</c> configuration element.
    /// </summary>
    public class TokenSignatureElement
    {
        #region Private Constants

        /// <summary>
        /// The pass phrase
        /// </summary>
        private const string PASS_PHRASE = "passPhrase";

        /// <summary>
        /// The signing certificate
        /// </summary>
        private const string SIGNING_CERTIFICATE = "signingCertificate";

        /// <summary>
        /// The signing certificate password
        /// </summary>
        private const string SIGNING_CERTIFICATE_PASSWORD = "signingCertificatePassword";

        /// <summary>
        /// The validating certificate
        /// </summary>
        private const string VALIDATING_CERTIFICATE = "validatingCertificate";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the pass phrase.
        /// </summary>
        /// <value>The pass phrase.</value>
        public string PassPhrase { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the signing certificate.
        /// </summary>
        /// <value>The signing certificate.</value>
        public string SigningCertificate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the signing certificate password.
        /// </summary>
        /// <value>The signing certificate password.</value>
        public string SigningCertificatePassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the validating certificate.
        /// </summary>
        /// <value>The validating certificate.</value>
        public string ValidatingCertificate { get; set; } = string.Empty;

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads a signature element from its configuration element.
        /// </summary>
        /// <param name="signatureElement">The signature element.</param>
        /// <returns>TokenSignatureElement.</returns>
        internal static TokenSignatureElement Load(XElement signatureElement)
        {
            if (signatureElement == null)
            {
                return null;
            }

            return new TokenSignatureElement
            {
                PassPhrase = (string)signatureElement.Attribute(PASS_PHRASE) ?? string.Empty,
                SigningCertificate = (string)signatureElement.Attribute(SIGNING_CERTIFICATE) ?? string.Empty,
                SigningCertificatePassword = (string)signatureElement.Attribute(SIGNING_CERTIFICATE_PASSWORD) ?? string.Empty,
                ValidatingCertificate = (string)signatureElement.Attribute(VALIDATING_CERTIFICATE) ?? string.Empty
            };
        }

        #endregion
    }
}
