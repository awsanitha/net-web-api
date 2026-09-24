namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenElement.
    /// POCO replacement for the former ConfigurationElement.
    /// </summary>
    public class TokenElement
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>The definition.</value>
        public TokenDefinitionElement Definition { get; set; }
    }
}
