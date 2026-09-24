using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenConfigurationSection.
    /// POCO replacement for the former ConfigurationSection.
    /// Loads token definitions from XML config files (token*.config).
    /// </summary>
    public class TokenConfigurationSection
    {
        #region Private Constants

        /// <summary>
        /// The token section name
        /// </summary>
        internal const string SECTION_NAME = "tokenSection";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        /// <value>The members.</value>
        public TokenElementCollection Members { get; set; } = new TokenElementCollection();

        #endregion

        #region Static Methods

        /// <summary>
        /// Loads a TokenConfigurationSection from an XML config file.
        /// </summary>
        /// <param name="configFilePath">The configuration file path.</param>
        /// <returns>TokenConfigurationSection.</returns>
        public static TokenConfigurationSection LoadFromFile(string configFilePath)
        {
            var section = new TokenConfigurationSection();

            if (!File.Exists(configFilePath))
            {
                return section;
            }

            var doc = XDocument.Load(configFilePath);
            var tokenSectionElement = doc.Root?.Element(SECTION_NAME);

            if (tokenSectionElement == null)
            {
                return section;
            }

            var tokensElement = tokenSectionElement.Element("tokens");

            if (tokensElement == null)
            {
                return section;
            }

            foreach (var tokenElement in tokensElement.Elements("token"))
            {
                var name = tokenElement.Attribute("name")?.Value;

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var definitionElement = tokenElement.Element("definition");

                if (definitionElement == null)
                {
                    continue;
                }

                var signatureElement = definitionElement.Element("signature");

                var definition = new TokenDefinitionElement
                {
                    Issuer = definitionElement.Attribute("issuer")?.Value ?? "http://no.where.com",
                    IntendedAudience = definitionElement.Attribute("intendedAudience")?.Value ?? "urn:no-where",
                    ExpirationInMinute = double.TryParse(definitionElement.Attribute("expirationInMinute")?.Value, out var exp) ? exp : 15d,
                    IsBase64Encoded = bool.TryParse(definitionElement.Attribute("isBase64Encoded")?.Value, out var b64) && b64,
                    OneTimeUse = bool.TryParse(definitionElement.Attribute("oneTimeUse")?.Value, out var otu) && otu,
                    Signature = signatureElement != null ? new TokenSignatureElement
                    {
                        PassPhrase = signatureElement.Attribute("passPhrase")?.Value ?? "",
                        SigningCertificate = signatureElement.Attribute("signingCertificate")?.Value ?? "",
                        SigningCertificatePassword = signatureElement.Attribute("signingCertificatePassword")?.Value ?? "",
                        ValidatingCertificate = signatureElement.Attribute("validatingCertificate")?.Value ?? ""
                    } : null
                };

                section.Members.Add(new TokenElement
                {
                    Name = name,
                    Definition = definition
                });
            }

            return section;
        }

        #endregion
    }
}
