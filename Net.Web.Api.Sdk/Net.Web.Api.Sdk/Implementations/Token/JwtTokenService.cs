using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Configurations.Token;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Implementations.Token
{
    /// <inheritdoc />
    public class JwtTokenService : IJwtTokenService
    {
        private const string TOKEN_CONFIG_FILE_PATTERN = "token*.config";
        private const string TOKEN_DATA_COLLECTION = "tokens";

        public Dictionary<string, JwtTokenModel> Tokens { get; private set; }

        private string _tokenDataBase;
        private readonly string _rootPath;

        public JwtTokenService(IWebHostEnvironment env)
        {
            _rootPath = env.ContentRootPath;
            LoadAllTokens();
            SetupTokenDatabase();
        }

        public virtual string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null)
        {
            if (string.IsNullOrEmpty(tokenName)) throw new ArgumentNullException(nameof(tokenName));

            tokenName = tokenName.ToUpper();
            if (!Tokens.ContainsKey(tokenName)) throw new KeyNotFoundException(tokenName);

            var definition = Tokens[tokenName];
            var now = DateTime.UtcNow;
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = definition.TokenIssuer,
                Audience = definition.TokenIntendedAudience,
                IssuedAt = now,
                Expires = now.AddMinutes(definition.TokenExpirationInMinutes),
                Subject = GetTokenSubject(definition, identityName, customClaims),
                SigningCredentials = definition.SigningTokenCredential.SigningCredentials
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            var generatedToken = handler.WriteToken(token);
            return FormatToken(generatedToken, definition);
        }

        public virtual TokenValidationParameters GetTokenValidationParameters(
            bool validateExipration = false,
            string issuers = null,
            string audiences = null)
        {
            var vi = !string.IsNullOrEmpty(issuers);
            var va = !string.IsNullOrEmpty(audiences);

            var parameters = new TokenValidationParameters
            {
                ValidateActor = false,
                RequireSignedTokens = true,
                RequireExpirationTime = validateExipration,
                ValidateLifetime = validateExipration,
                ValidateIssuer = vi,
                ValidateAudience = va,
                ValidateIssuerSigningKey = false,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeys = GetAllSecurityKeys()
            };

            if (vi) parameters.ValidIssuers = issuers.Split(',').Select(p => p.Trim()).ToList();
            if (va) parameters.ValidAudiences = audiences.Split(',').Select(p => p.Trim()).ToList();

            return parameters;
        }

        public virtual Dictionary<string, string> GetTokenPayload(HttpContext context)
        {
            var identity = context?.User?.Identity;
            if (identity == null || !identity.IsAuthenticated) return new Dictionary<string, string>();

            context.Request.GetToken(out var securityToken);
            return securityToken?.Claims?.ToClaimDictionary() ?? new Dictionary<string, string>();
        }

        public virtual Dictionary<string, string> GetIdentityPayload(HttpContext context)
        {
            var identity = context?.User?.Identity;
            if (identity == null || !identity.IsAuthenticated) return new Dictionary<string, string>();
            var claims = ((ClaimsIdentity)identity).Claims;
            return claims?.ToClaimDictionary(true) ?? new Dictionary<string, string>();
        }

        public virtual bool IsTokenRevoked(string token, List<Claim> claims)
        {
            return GetTokenState(token, claims, out var isRevoked, out _) && isRevoked;
        }

        public virtual bool IsTokenUsed(string token, List<Claim> claims)
        {
            if (!claims.IsTokenOneTimeUse()) return false;
            return GetTokenState(token, claims, out _, out var isUsed) && isUsed;
        }

        public virtual bool RevokeToken(string token, List<Claim> claims)
        {
            if (GetTokenState(token, claims, out _, out _)) return false;
            return RevokeTokenInternal(token);
        }

        public virtual void MarkTokenAsUsed(string token, List<Claim> claims)
        {
            if (!claims.IsTokenOneTimeUse() || GetTokenState(token, claims, out _, out _)) return;
            MarkTokenAsUsedInternal(token);
        }

        public virtual int CleanupTokenDatabase()
        {
            using var db = new LiteDatabase(_tokenDataBase);
            var col = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
            var now = DateTime.UtcNow;
            return col.DeleteMany(c => c.ExpirationDate < now);
        }

        private bool RevokeTokenInternal(string token)
        {
            using var db = new LiteDatabase(_tokenDataBase);
            var col = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
            if (col.FindOne(c => c.Token == token) == null)
            {
                col.Insert(new JwtTokenUsedOrRevoked
                {
                    Token = token,
                    IsRevoked = true,
                    ExpirationDate = token.GetExpirationDate(),
                    RevocationDate = DateTime.UtcNow
                });
            }
            return true;
        }

        private void MarkTokenAsUsedInternal(string token)
        {
            using var db = new LiteDatabase(_tokenDataBase);
            var col = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
            if (col.FindOne(c => c.Token == token) == null)
            {
                col.Insert(new JwtTokenUsedOrRevoked
                {
                    Token = token,
                    IsUsed = true,
                    ExpirationDate = token.GetExpirationDate(),
                    UsedDate = DateTime.UtcNow
                });
            }
        }

        private bool GetTokenState(string token, List<Claim> claims, out bool isRevoked, out bool isUsed)
        {
            isRevoked = false;
            isUsed = false;
            if (string.IsNullOrEmpty(token) || claims == null || !claims.Any()) return false;

            using var db = new LiteDatabase(_tokenDataBase);
            var col = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
            var found = col.FindOne(c => c.Token == token);
            if (found == null) return false;

            isUsed = found.IsUsed;
            isRevoked = found.IsRevoked;
            return true;
        }

        private IEnumerable<SecurityKey> GetAllSecurityKeys()
        {
            return Tokens.Values
                .Where(t => t.ValidatingTokenCredential?.SecurityKey != null)
                .Select(t => t.ValidatingTokenCredential.SecurityKey)
                .ToList();
        }

        private static ClaimsIdentity GetTokenSubject(JwtTokenModel definition, string identityName,
            IReadOnlyDictionary<string, string> customClaims)
        {
            var identity = new ClaimsIdentity("Bearer");
            identity.AddClaim(new Claim(ClaimTypes.Name, identityName));
            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, identityName));
            identity.AddClaim(new Claim(TokenInternalClaimNames.tn.ToString(), definition.TokenName));
            identity.AddClaim(new Claim(TokenInternalClaimNames.jti.ToString(), Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(TokenInternalClaimNames.exm.ToString(),
                definition.TokenExpirationInMinutes.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(TokenInternalClaimNames.otu.ToString(),
                definition.OneTimeUse.ToString().ToLower()));

            if (customClaims == null || customClaims.Count == 0) return identity;

            var internals = Enum.GetValues(typeof(TokenInternalClaimNames))
                .Cast<TokenInternalClaimNames>().Select(v => v.ToString()).ToList();

            foreach (var claim in customClaims)
            {
                if (!internals.Contains(claim.Key))
                    identity.AddClaim(new Claim(claim.Key, claim.Value));
            }

            return identity;
        }

        private static string FormatToken(string token, JwtTokenModel definition)
        {
            return !definition.IsTokenBase64Encoded
                ? token
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(token)).ToSecuredEncoded64Padding();
        }

        private static Dictionary<string, JwtTokenModel> LoadTokenList(TokenConfigurationSection section, string rootPath)
        {
            var tokens = new Dictionary<string, JwtTokenModel>();
            for (var i = 0; i < section.Members.Count; i++)
            {
                var element = section.Members[i];
                if (element.Definition?.Signature == null || tokens.ContainsKey(element.Name)) continue;
                tokens.Add(element.Name, new JwtTokenModel(element.Name, element.Definition, rootPath));
            }
            return tokens;
        }

        private void LoadAllTokens()
        {
            Tokens = new Dictionary<string, JwtTokenModel>();
            var configFiles = Directory.GetFiles(_rootPath, TOKEN_CONFIG_FILE_PATTERN, SearchOption.AllDirectories);
            if (!configFiles.Any()) return;

            foreach (var configFile in configFiles)
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                var section = (TokenConfigurationSection)config.GetSection(TokenConfigurationSection.SECTION_NAME);
                if (section == null) continue;

                foreach (var token in LoadTokenList(section, _rootPath))
                {
                    if (!Tokens.ContainsKey(token.Key))
                        Tokens.Add(token.Key, token.Value);
                    else
                        Tokens[token.Key] = token.Value;
                }
            }
        }

        private void SetupTokenDatabase()
        {
            var dbPath = Path.Combine(_rootPath, "db");
            if (!Directory.Exists(dbPath)) Directory.CreateDirectory(dbPath);
            _tokenDataBase = Path.Combine(dbPath, "TokenDataBase.db");

            var mapper = BsonMapper.Global;
            mapper.Entity<JwtTokenUsedOrRevoked>().Id(c => c.Id);
        }
    }
}
