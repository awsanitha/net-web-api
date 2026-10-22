using System.Collections.Generic;
using System.Security.Claims;
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
        Dictionary<string, JwtTokenModel> Tokens { get; }

        /// <summary>
        /// Creates the token.
        /// </summary>
        string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null);

        /// <summary>
        /// Gets the token validation parameters.
        /// </summary>
        TokenValidationParameters GetTokenValidationParameters(bool validateExipration = false, string issuers = null, string audiences = null);

        /// <summary>
        /// Gets the token payload from the JWT token string.
        /// </summary>
        Dictionary<string, string> GetTokenPayload(string token);

        /// <summary>
        /// Gets the identity payload from claims principal.
        /// </summary>
        Dictionary<string, string> GetIdentityPayload(ClaimsPrincipal principal);

        /// <summary>
        /// Determines whether the token has been revoked.
        /// </summary>
        bool IsTokenRevoked(string token, List<Claim> claims);

        /// <summary>
        /// Determines whether the token has been used.
        /// </summary>
        bool IsTokenUsed(string token, List<Claim> claims);

        /// <summary>
        /// Revokes the token.
        /// </summary>
        bool RevokeToken(string token, List<Claim> claims);

        /// <summary>
        /// Marks the token as used.
        /// </summary>
        void MarkTokenAsUsed(string token, List<Claim> claims);

        /// <summary>
        /// Cleans up expired tokens from the database.
        /// </summary>
        int CleanupTokenDatabase();
    }
}
