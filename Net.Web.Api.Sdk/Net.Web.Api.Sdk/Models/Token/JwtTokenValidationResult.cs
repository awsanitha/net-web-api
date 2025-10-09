using System.Collections.Generic;
using System.Security.Claims;

namespace Net.Web.Api.Sdk.Models.Token
{
    /// <summary>
    /// Class JwtTokenValidationResult.
    /// </summary>
    public class JwtTokenValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>The claims.</value>
        public IEnumerable<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        public string Token { get; set; }
    }
}
