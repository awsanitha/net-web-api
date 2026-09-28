namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenConfiguration.
    /// Options POCO replacing the former ConfigurationSection.
    /// </summary>
    public class TokenConfiguration
    {
        /// <summary>
        /// The configuration section name used for IOptions binding.
        /// </summary>
        public const string SectionName = "TokenConfiguration";

        /// <summary>
        /// Gets or sets the list of token definitions.
        /// </summary>
        /// <value>The tokens.</value>
        public List<TokenOptions> Tokens { get; set; } = new();
    }
}
