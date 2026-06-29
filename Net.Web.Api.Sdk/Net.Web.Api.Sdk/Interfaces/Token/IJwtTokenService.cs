using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Injection.Attributes;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Interfaces.Token
{
    /// <summary>
    /// JWT token service.
    /// </summary>
    [InjectInterfaceService]
    public interface IJwtTokenService
    {
        Dictionary<string, JwtTokenModel> Tokens { get; }

        string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null);

        TokenValidationParameters GetTokenValidationParameters(bool validateExipration = false, string issuers = null, string audiences = null);

        Dictionary<string, string> GetTokenPayload(HttpContext context);

        Dictionary<string, string> GetIdentityPayload(HttpContext context);

        bool IsTokenRevoked(string token, List<Claim> claims);

        bool IsTokenUsed(string token, List<Claim> claims);

        bool RevokeToken(string token, List<Claim> claims);

        void MarkTokenAsUsed(string token, List<Claim> claims);

        int CleanupTokenDatabase();
    }
}
