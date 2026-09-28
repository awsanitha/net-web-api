namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenOptions.
    /// Options POCO replacing the former ConfigurationElement.
    /// </summary>
    public class TokenOptions
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>The definition.</value>
        public TokenDefinitionOptions Definition { get; set; } = new();
    }
}
