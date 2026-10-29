using System;
using System.Linq;
using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenElement.
    /// Plain object representation of a <c>token</c> configuration element.
    /// </summary>
    public class TokenElement
    {
        #region Private Constants

        /// <summary>
        /// The token name
        /// </summary>
        private const string TOKEN_NAME = "name";

        /// <summary>
        /// The token definition
        /// </summary>
        private const string TOKEN_DEFINITION = "definition";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        /// <value>The definition.</value>
        public TokenDefinitionElement Definition { get; internal set; }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads a token element from its configuration element.
        /// </summary>
        /// <param name="tokenElement">The token element.</param>
        /// <returns>TokenElement.</returns>
        internal static TokenElement Load(XElement tokenElement)
        {
            if (tokenElement == null)
            {
                return null;
            }

            var definitionElement = tokenElement.Elements()
                .FirstOrDefault(element => element.Name.LocalName.Equals(TOKEN_DEFINITION, StringComparison.OrdinalIgnoreCase));

            return new TokenElement
            {
                Name = (string)tokenElement.Attribute(TOKEN_NAME),
                Definition = TokenDefinitionElement.Load(definitionElement)
            };
        }

        #endregion
    }
}
