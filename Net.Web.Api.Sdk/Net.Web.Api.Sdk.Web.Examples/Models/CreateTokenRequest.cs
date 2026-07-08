using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Net.Web.Api.Sdk.Attributes.Validations;

namespace Net.Web.Api.Sdk.Web.Examples.Models
{
    /// <summary>
    /// Class CreateTokenRequest. Request model for token creation.
    /// </summary>
    public class CreateTokenRequest
    {
        #region Public Properties

        /// <summary>
        /// The token name as defined in the token configuration files.
        ///     - Case insensitive.
        /// </summary>
        [Required]
        [StringLength(16)]
        [TokenNameExists]
        public string? Name { get; set; }

        /// <summary>
        /// The token unique identifier that will be used as identity name.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string? UniqueId { get; set; }

        /// <summary>
        /// The token payload.
        /// </summary>
        [TokenPayloadValid]
        public List<KeyValuePair<string, string>>? Payload { get; set; }

        #endregion
    }
}
