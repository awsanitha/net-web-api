using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Injection.Attributes;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Interfaces.Token
{
    /// <summary>
    /// Interface IJwtTokenService
    /// </summary>
    [InjectInterfaceService]
    public interface IJwtTokenService
    {
        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <value>The tokens.</value>
        Dictionary<string, JwtTokenModel> Tokens { get; }

        /// <summary>
        /// Creates the token.
        /// </summary>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="identityName">Name of the identity.</param>
        /// <param name="customClaims">The custom claims.</param>
        /// <returns>System.String.</returns>
        string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null);

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <param name="validationRequest">The validation request parameters.</param>
        /// <returns>JwtTokenValidationResult.</returns>
        JwtTokenValidationResult ValidateToken(string token, JwtTokenValidationRequest validationRequest);

        /// <summary>
        /// Gets the token validation parameters.
        /// </summary>
        /// <param name="validateExpiration">if set to <c>true</c> [validate expiration].</param>
        /// <param name="issuers">The issuers.</param>
        /// <param name="audiences">The audiences.</param>
        /// <returns>TokenValidationParameters.</returns>
        TokenValidationParameters GetTokenValidationParameters(bool validateExpiration = false, string issuers = null, string audiences = null);

        /// <summary>
        /// Gets the token payload.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        Dictionary<string, string> GetTokenPayload(HttpContext httpContext);

        /// <summary>
        /// Gets the identity payload.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        Dictionary<string, string> GetIdentityPayload(HttpContext httpContext);

        /// <summary>
        /// Determines whether [is token revoked] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="claims">The claims.</param>
        /// <returns><c>true</c> if [is token revoked] [the specified token]; otherwise, <c>false</c>.</returns>
        bool IsTokenRevoked(string token, List<Claim> claims);

        /// <summary>
        /// Determines whether [is token used] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="claims">The claims.</param>
        /// <returns><c>true</c> if [is token used] [the specified token]; otherwise, <c>false</c>.</returns>
        bool IsTokenUsed(string token, List<Claim> claims);

        /// <summary>
        /// Revokes the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="claims">The claims.</param>
        bool RevokeToken(string token, List<Claim> claims);

        /// <summary>
        /// Marks the token as used.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="claims">The claims.</param>
        void MarkTokenAsUsed(string token, List<Claim> claims);

        /// <summary>
        /// Cleanups the token database.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int CleanupTokenDatabase();
    }
}
