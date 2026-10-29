using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenConfigurationSection.
    /// Plain object representation of the legacy <c>tokenSection</c> configuration section. The XML
    /// shape of the <c>token*.config</c> files is unchanged; only the reader changed, because the
    /// <c>System.Configuration</c> section model is not available on modern .NET.
    /// </summary>
    public class TokenConfigurationSection
    {
        #region Private Constants

        /// <summary>
        /// The token section name
        /// </summary>
        internal const string SECTION_NAME = "tokenSection";

        /// <summary>
        /// The collection name
        /// </summary>
        private const string COLLECTION_NAME = "tokens";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        public TokenElementCollection Members { get; set; } = new TokenElementCollection();

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the token section from a configuration file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>TokenConfigurationSection, or <c>null</c> when the file holds no token section.</returns>
        public static TokenConfigurationSection Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }

            var document = XDocument.Load(fileName);
            var sectionElement = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName.Equals(SECTION_NAME, StringComparison.OrdinalIgnoreCase));

            if (sectionElement == null)
            {
                return null;
            }

            var section = new TokenConfigurationSection();
            var collectionElement = sectionElement.Elements()
                .FirstOrDefault(element => element.Name.LocalName.Equals(COLLECTION_NAME, StringComparison.OrdinalIgnoreCase));

            if (collectionElement == null)
            {
                return section;
            }

            section.Members = TokenElementCollection.Load(collectionElement);

            return section;
        }

        #endregion
    }
}
