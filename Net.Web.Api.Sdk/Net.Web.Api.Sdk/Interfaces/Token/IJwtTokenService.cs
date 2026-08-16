using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Injection.Attributes;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Interfaces.Token
{
    /// <summary>
    /// Service interface for JWT token creation, validation, and lifecycle management.
    /// </summary>
    [InjectInterfaceService]
    public interface IJwtTokenService
    {
        /// <summary>Gets the loaded token definitions keyed by token name (upper-case).</summary>
        Dictionary<string, JwtTokenModel> Tokens { get; }

        /// <summary>Creates a signed JWT token.</summary>
        string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null);

        /// <summary>Builds token-validation parameters, optionally enforcing expiry, issuers and audiences.</summary>
        TokenValidationParameters GetTokenValidationParameters(
            bool validateExipration = false,
            string issuers = null,
            string audiences = null);

        /// <summary>Extracts the token payload claims from the current request context.</summary>
        Dictionary<string, string> GetTokenPayload(ActionContext context);

        /// <summary>Extracts the identity claims from the current request context.</summary>
        Dictionary<string, string> GetIdentityPayload(ActionContext context);

        /// <summary>Returns <c>true</c> when the token has been explicitly revoked.</summary>
        bool IsTokenRevoked(string token, List<Claim> claims);

        /// <summary>Returns <c>true</c> when a one-time-use token has already been consumed.</summary>
        bool IsTokenUsed(string token, List<Claim> claims);

        /// <summary>Revokes the token so it will no longer be accepted.</summary>
        bool RevokeToken(string token, List<Claim> claims);

        /// <summary>Marks a one-time-use token as consumed.</summary>
        void MarkTokenAsUsed(string token, List<Claim> claims);

        /// <summary>Removes expired token records from the token database.</summary>
        int CleanupTokenDatabase();
    }
}
