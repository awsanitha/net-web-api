using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenDefinitionElement.
    /// Plain object representation of a <c>definition</c> configuration element.
    /// </summary>
    public class TokenDefinitionElement
    {
        #region Private Constants

        /// <summary>
        /// The issuer
        /// </summary>
        private const string ISSUER = "issuer";

        /// <summary>
        /// The intended audience
        /// </summary>
        private const string INTENDED_AUDIENCE = "intendedAudience";

        /// <summary>
        /// The expiration in minute
        /// </summary>
        private const string EXPIRATION_IN_MINUTE = "expirationInMinute";

        /// <summary>
        /// The is base 64 encoded
        /// </summary>
        private const string IS_BASE_64_ENCODED = "isBase64Encoded";

        /// <summary>
        /// The one time use
        /// </summary>
        private const string ONE_TIME_USE = "oneTimeUse";

        /// <summary>
        /// The signature
        /// </summary>
        private const string SIGNATURE = "signature";

        #endregion

        #region Public Properties

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
        public TokenSignatureElement Signature { get; internal set; } = new TokenSignatureElement();

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads a definition element from its configuration element.
        /// </summary>
        /// <param name="definitionElement">The definition element.</param>
        /// <returns>TokenDefinitionElement.</returns>
        internal static TokenDefinitionElement Load(XElement definitionElement)
        {
            if (definitionElement == null)
            {
                return null;
            }

            var definition = new TokenDefinitionElement();
            var issuer = (string)definitionElement.Attribute(ISSUER);

            if (!string.IsNullOrEmpty(issuer))
            {
                definition.Issuer = issuer;
            }

            var intendedAudience = (string)definitionElement.Attribute(INTENDED_AUDIENCE);

            if (!string.IsNullOrEmpty(intendedAudience))
            {
                definition.IntendedAudience = intendedAudience;
            }

            var expiration = (string)definitionElement.Attribute(EXPIRATION_IN_MINUTE);

            if (!string.IsNullOrEmpty(expiration)
                && double.TryParse(expiration, NumberStyles.Float, CultureInfo.InvariantCulture, out var expirationValue))
            {
                definition.ExpirationInMinute = expirationValue;
            }

            definition.IsBase64Encoded = ReadBoolean(definitionElement, IS_BASE_64_ENCODED);
            definition.OneTimeUse = ReadBoolean(definitionElement, ONE_TIME_USE);

            var signatureElement = definitionElement.Elements()
                .FirstOrDefault(element => element.Name.LocalName.Equals(SIGNATURE, StringComparison.OrdinalIgnoreCase));

            definition.Signature = TokenSignatureElement.Load(signatureElement);

            return definition;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads a boolean attribute, defaulting to <c>false</c> when absent or unparsable.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns><c>true</c> when the attribute reads as true; otherwise, <c>false</c>.</returns>
        private static bool ReadBoolean(XElement element, string attributeName)
        {
            var value = (string)element.Attribute(attributeName);

            return !string.IsNullOrEmpty(value) && bool.TryParse(value, out var result) && result;
        }

        #endregion
    }
}
