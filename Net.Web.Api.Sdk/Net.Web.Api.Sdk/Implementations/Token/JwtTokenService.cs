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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Configurations.Token;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Models.Token;

namespace Net.Web.Api.Sdk.Implementations.Token
{
    /// <inheritdoc />
    /// <summary>
    /// Class JwtTokenService.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        #region Constants

        /// <summary>
        /// The token config file pattern
        /// </summary>
        private const string TOKEN_CONFIG_FILE_PATTERN = "token*.config";

        /// <summary>
        /// The token data collection
        /// </summary>
        private const string TOKEN_DATA_COLLECTION = "tokens";

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public Dictionary<string, JwtTokenModel> Tokens { get; private set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The token data base path
        /// </summary>
        private string _tokenDataBase;

        /// <summary>
        /// The root path
        /// </summary>
        private readonly string _rootPath;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
        /// </summary>
        /// <param name="hostEnvironment">The host environment.</param>
        public JwtTokenService(IHostEnvironment hostEnvironment)
        {
            _rootPath = hostEnvironment?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
            LoadAllTokens();
            SetupTokenDatabase();
        }

        #endregion

        #region IJwtTokenService Implementations

        /// <inheritdoc />
        public virtual string CreateToken(string tokenName, string identityName, Dictionary<string, string> customClaims = null)
        {
            if (string.IsNullOrEmpty(tokenName))
            {
                throw new ArgumentNullException(nameof(tokenName));
            }

            tokenName = tokenName.ToUpper();

            var isTokenDefined = Tokens.ContainsKey(tokenName);

            if (!isTokenDefined)
            {
                throw new KeyNotFoundException(tokenName);
            }

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

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            var generatedToken = tokenHandler.WriteToken(token);

            return FormatToken(generatedToken, definition);
        }

        /// <inheritdoc />
        public virtual TokenValidationParameters GetTokenValidationParameters(
            bool validateExipration = false,
            string issuers = null,
            string audiences = null)
        {
            var vi = !string.IsNullOrEmpty(issuers);
            var va = !string.IsNullOrEmpty(audiences);

            var validationParameters = new TokenValidationParameters
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

            if (vi)
            {
                validationParameters.ValidIssuers = issuers.Split(',').Select(p => p.Trim()).ToList();
            }

            if (va)
            {
                validationParameters.ValidAudiences = audiences.Split(',').Select(p => p.Trim()).ToList();
            }

            return validationParameters;
        }

        /// <inheritdoc />
        public virtual Dictionary<string, string> GetTokenPayload(ActionContext context)
        {
            var identity = context?.HttpContext?.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return new Dictionary<string, string>();
            }

            var token = context.HttpContext.GetBearerToken(out var securityToken);

            return securityToken?.Claims?.ToClaimDictionary() ?? new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public virtual Dictionary<string, string> GetIdentityPayload(ActionContext context)
        {
            var principal = context?.HttpContext?.User;
            var identity = principal?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return new Dictionary<string, string>();
            }

            var claims = ((ClaimsIdentity)identity).Claims;

            return claims?.ToClaimDictionary(true) ?? new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public virtual bool IsTokenRevoked(string token, List<Claim> claims)
        {
            var found = GetTokenState(token, claims, out var isRevoked, out _);

            if (!found)
            {
                return false;
            }

            return isRevoked;
        }

        /// <inheritdoc />
        public virtual bool IsTokenUsed(string token, List<Claim> claims)
        {
            if (!claims.IsTokenOneTimeUse())
            {
                return false;
            }

            var found = GetTokenState(token, claims, out _, out var isUsed);

            if (!found)
            {
                return false;
            }

            return isUsed;
        }

        /// <inheritdoc />
        public virtual bool RevokeToken(string token, List<Claim> claims)
        {
            var found = GetTokenState(token, claims, out _, out _);

            if (found)
            {
                return false;
            }

            return RevokeTokenInternal(token);
        }

        /// <inheritdoc />
        public virtual void MarkTokenAsUsed(string token, List<Claim> claims)
        {
            if (!claims.IsTokenOneTimeUse())
            {
                return;
            }

            var found = GetTokenState(token, claims, out _, out _);

            if (found)
            {
                return;
            }

            MarkTokenAsUsedInternal(token);
        }

        /// <inheritdoc />
        public virtual int CleanupTokenDatabase()
        {
            var count = 0;

            using (var db = new LiteDatabase(_tokenDataBase))
            {
                var tokens = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
                var now = DateTime.UtcNow;

                count = tokens.DeleteMany(c => c.ExpirationDate.CompareTo(now) > 0);
            }

            return count;
        }

        #endregion

        #region Private Methods

        private bool RevokeTokenInternal(string token)
        {
            using (var db = new LiteDatabase(_tokenDataBase))
            {
                var tokens = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
                var found = tokens.Find(c => c.Token.Equals(token)).FirstOrDefault();

                if (found == null)
                {
                    var record = new JwtTokenUsedOrRevoked
                    {
                        Token = token,
                        IsRevoked = true,
                        ExpirationDate = token.GetExpirationDate(),
                        RevocationDate = DateTime.UtcNow
                    };

                    tokens.Insert(record);
                }
            }

            return true;
        }

        private void MarkTokenAsUsedInternal(string token)
        {
            using (var db = new LiteDatabase(_tokenDataBase))
            {
                var tokens = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
                var found = tokens.Find(c => c.Token.Equals(token)).FirstOrDefault();

                if (found == null)
                {
                    var record = new JwtTokenUsedOrRevoked
                    {
                        Token = token,
                        IsUsed = true,
                        ExpirationDate = token.GetExpirationDate(),
                        UsedDate = DateTime.UtcNow
                    };

                    tokens.Insert(record);
                }
            }
        }

        private bool GetTokenState(string token, List<Claim> claims, out bool isRevoked, out bool isUsed)
        {
            isRevoked = false;
            isUsed = false;

            if (string.IsNullOrEmpty(token) || claims == null || !claims.Any())
            {
                return false;
            }

            JwtTokenUsedOrRevoked found = null;

            using (var db = new LiteDatabase(_tokenDataBase))
            {
                var tokens = db.GetCollection<JwtTokenUsedOrRevoked>(TOKEN_DATA_COLLECTION);
                found = tokens.Find(c => c.Token.Equals(token)).FirstOrDefault();

                if (found != null)
                {
                    isUsed = found.IsUsed;
                    isRevoked = found.IsRevoked;
                }
            }

            return found != null;
        }

        private IEnumerable<SecurityKey> GetAllSecurityKeys()
        {
            return (from td in Tokens
                    where td.Value.ValidatingTokenCredential?.SecurityKey != null
                    select td.Value.ValidatingTokenCredential.SecurityKey).ToList();
        }

        private static ClaimsIdentity GetTokenSubject(JwtTokenModel definition,
            string identityName,
            IReadOnlyDictionary<string, string> customClaims)
        {
            var identity = new ClaimsIdentity("Bearer");

            identity.AddClaim(new Claim(ClaimTypes.Name, identityName));
            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, identityName));
            identity.AddClaim(new Claim(TokenInternalClaimNames.tn.ToString(), definition.TokenName));
            identity.AddClaim(new Claim(TokenInternalClaimNames.jti.ToString(), Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(TokenInternalClaimNames.exm.ToString(), definition.TokenExpirationInMinutes.ToString(CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(TokenInternalClaimNames.otu.ToString(), definition.OneTimeUse.ToString().ToLower()));

            if (customClaims == null || customClaims.Count <= 0)
            {
                return identity;
            }

            var internals = Enum.GetValues(typeof(TokenInternalClaimNames)).Cast<TokenInternalClaimNames>().Select(v => v.ToString()).ToList();

            foreach (var claim in customClaims)
            {
                if (internals.Contains(claim.Key))
                {
                    continue;
                }

                identity.AddClaim(new Claim(claim.Key, claim.Value));
            }

            return identity;
        }

        private static string FormatToken(string generatedToken, JwtTokenModel definition)
        {
            return !definition.IsTokenBase64Encoded
                ? generatedToken
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(generatedToken)).ToSecuredEncoded64Padding();
        }

        private static Dictionary<string, JwtTokenModel> LoadTokenList(TokenConfigurationSection tokenConfigurationSection, string rootPath)
        {
            var tokens = new Dictionary<string, JwtTokenModel>();

            for (var i = 0; i < tokenConfigurationSection.Members.Count; i++)
            {
                var definition = tokenConfigurationSection.Members[i].Definition;
                var tokenName = tokenConfigurationSection.Members[i].Name;

                if (definition?.Signature == null || tokens.ContainsKey(tokenName))
                {
                    continue;
                }

                tokens.Add(tokenName, new JwtTokenModel(tokenName, definition, rootPath));
            }

            return tokens;
        }

        private void LoadAllTokens()
        {
            Tokens = new Dictionary<string, JwtTokenModel>();

            var configurationFileList = Directory.GetFiles(_rootPath, TOKEN_CONFIG_FILE_PATTERN, SearchOption.AllDirectories);

            if (!configurationFileList.Any())
            {
                return;
            }

            foreach (var configurationFile in configurationFileList)
            {
                var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configurationFile };
                var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                var tokenSection = (TokenConfigurationSection)config.GetSection(TokenConfigurationSection.SECTION_NAME);

                if (tokenSection == null) continue;

                var tokenList = LoadTokenList(tokenSection, _rootPath);

                foreach (var token in tokenList)
                {
                    if (!Tokens.ContainsKey(token.Key))
                    {
                        Tokens.Add(token.Key, token.Value);
                    }
                    else
                    {
                        Tokens[token.Key] = token.Value;
                    }
                }
            }
        }

        private void SetupTokenDatabase()
        {
            var dataBasePath = Path.Combine(_rootPath, "db");

            if (!Directory.Exists(dataBasePath))
            {
                Directory.CreateDirectory(dataBasePath);
            }

            _tokenDataBase = Path.Combine(dataBasePath, "TokenDataBase.db");

            var mapper = BsonMapper.Global;

            mapper.Entity<JwtTokenUsedOrRevoked>().Id(c => c.Id);
        }

        #endregion
    }
}
