using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Configurations.Token;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Net.Web.Api.Sdk.Models.Token
{
    internal enum TokenInternalClaimNames
    {
        /// <summary>
        /// The token will not be valid before a give utc date/time
        /// </summary>
        nbf,

        /// <summary>
        /// The token expiration utc date/time
        /// </summary>
        exp,

        /// <summary>
        /// The token issue utc date/time
        /// </summary>
        iat,

        /// <summary>
        /// The token issuer
        /// </summary>
        iss,

        /// <summary>
        /// The token audience
        /// </summary>
        aud,

        /// <summary>
        /// The token unique identifier
        /// </summary>
        jti,

        /// <summary>
        /// The the token expiration in minutes
        /// </summary>
        exm,

        /// <summary>
        /// The token name
        /// </summary>
        tn,

        /// <summary>
        /// One time use token
        /// </summary>
        otu
    }

    /// <summary>
    /// Enum TokenSecurityType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenSecurityTypes
    {
        /// <summary>
        /// The pass phrase
        /// </summary>
        PassPhrase,

        /// <summary>
        /// The certificate
        /// </summary>
        Certificate
    }

    /// <summary>
    /// Enum TokenSecurityAlgorithms
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenSecurityAlgorithms
    {
        /// <summary>
        /// The hmac sha256
        /// </summary>
        HmacSha256,

        /// <summary>
        /// The hmac sha384
        /// </summary>
        HmacSha384,

        /// <summary>
        /// The hmac sha512
        /// </summary>
        HmacSha512
    }

    /// <summary>
    /// Enum TokenStatus
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenStatus
    {
        /// <summary>
        /// The already used
        /// </summary>
        AlreadyUsed,

        /// <summary>
        /// The expired
        /// </summary>
        Expired,

        /// <summary>
        /// The invalid
        /// </summary>
        Invalid,

        /// <summary>
        /// The invalid audience
        /// </summary>
        InvalidAudience,

        /// <summary>
        /// The required
        /// </summary>
        TokenRequired,

        /// <summary>
        /// The revoked
        /// </summary>
        Revoked,

        /// <summary>
        /// The valid
        /// </summary>
        Valid
    }

    /// <summary>
    /// Interface ITokenCredential
    /// </summary>
    internal interface ITokenCredential
    {
        /// <summary>
        /// Gets or sets the security key.
        /// </summary>
        /// <value>The security key.</value>
        SecurityKey? SecurityKey { get; set; }

        /// <summary>
        /// Gets or sets the signing credentials.
        /// </summary>
        /// <value>The signing credentials.</value>
        SigningCredentials? SigningCredentials { get; set; }
    }

    /// <summary>
    /// Class JwtTokenModel.
    /// </summary>
    public class JwtTokenModel
    {
        #region Private Constants

        /// <summary>
        /// The urn pattern
        /// </summary>
        private const string URN_PATTERN = @"^(?<URN>[uU][rR][nN]\:(?<NID>(?!urn\:)[a-zA-Z0-9][a-zA-Z0-9-]{1,31})\:(?<NSS>([a-zA-Z0-9()+,._!*':=@;$-]|%[0-9a-fA-F]{2})+))$";

        #endregion

        #region Public Properties

        /// <summary>
        /// The token name.
        /// </summary>
        public string TokenName { get; }

        /// <summary>
        /// The token Issuer.
        /// </summary>
        public string TokenIssuer { get; }

        /// <summary>
        /// The token intended audience.
        /// </summary>
        public string TokenIntendedAudience { get; }

        /// <summary>
        /// The token expiration in minutes.
        /// </summary>
        public double TokenExpirationInMinutes { get; }

        /// <summary>
        /// Specifies if the resulting token will be base 64 encoded.
        /// </summary>
        public bool IsTokenBase64Encoded { get; }

        /// <summary>
        /// Gets a value indicating whether [one time use].
        /// </summary>
        public bool OneTimeUse { get; }

        /// <summary>
        /// The token security type.
        /// </summary>
        public TokenSecurityTypes TokenSecurityType { get; }

        /// <summary>
        /// The token security algorithm.
        /// </summary>
        public TokenSecurityAlgorithms? TokenSecurityAlgorithm { get; set; }

        /// <summary>
        /// The signing token credential.
        /// </summary>
        [JsonIgnore]
        internal ITokenCredential? SigningTokenCredential { get; set; }

        /// <summary>
        /// The validating token credential.
        /// </summary>
        [JsonIgnore]
        internal ITokenCredential? ValidatingTokenCredential { get; set; }

        /// <summary>
        /// Token certificate algorithm.
        /// </summary>
        public string? TokenCertificateAlgorithm { get; internal set; }

        /// <summary>
        /// The certificate algorithm.
        /// </summary>
        public string? CertificateAlgorithm { get; internal set; }

        #endregion

        #region Conditional Serializations

        public bool ShouldSerializeTokenSecurityAlgorithm() => TokenSecurityAlgorithm.HasValue;

        public bool ShouldSerializeTokenCertificateAlgorithm() => !string.IsNullOrEmpty(TokenCertificateAlgorithm);

        public bool ShouldSerializeCertificateAlgorithm() => !string.IsNullOrEmpty(CertificateAlgorithm);

        #endregion

        #region Internal Class

        internal class TokenCredential : ITokenCredential
        {
            public SecurityKey? SecurityKey { get; set; }
            public SigningCredentials? SigningCredentials { get; set; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenModel"/> class.
        /// </summary>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="rootPath">The root path for certificate file resolution.</param>
        [SuppressMessage("ReSharper", "NotResolvedInText")]
        public JwtTokenModel(string tokenName, TokenDefinitionElement definition, string rootPath)
        {
            TokenName = tokenName.ToUpper();

            var validIssuer = Uri.TryCreate(definition.Issuer, UriKind.Absolute, out var uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!validIssuer)
            {
                throw new ArgumentException(definition.Issuer);
            }

            TokenIssuer = definition.Issuer;

            if (!Regex.IsMatch(definition.IntendedAudience, URN_PATTERN, RegexOptions.IgnorePatternWhitespace))
            {
                throw new ArgumentException(definition.IntendedAudience);
            }

            TokenIntendedAudience = definition.IntendedAudience;
            TokenExpirationInMinutes = definition.ExpirationInMinute;
            IsTokenBase64Encoded = definition.IsBase64Encoded;
            OneTimeUse = definition.OneTimeUse;

            var passPhrase = definition.Signature.PassPhrase;

            if (!string.IsNullOrEmpty(passPhrase))
            {
                TokenSecurityType = TokenSecurityTypes.PassPhrase;
                SetPassPhraseSignature(passPhrase);
                return;
            }

            TokenSecurityAlgorithm = null;

            if (string.IsNullOrEmpty(definition.Signature.ValidatingCertificate) && string.IsNullOrEmpty(definition.Signature.SigningCertificate))
            {
                throw new ArgumentNullException("ValidatingCertificate");
            }

            byte[]? validatingContent = null;
            byte[]? signingContent = null;

            if (!string.IsNullOrEmpty(definition.Signature.ValidatingCertificate))
            {
                var validationCertificateFile = SearchCertificate(definition.Signature.ValidatingCertificate, rootPath);

                if (string.IsNullOrEmpty(validationCertificateFile))
                {
                    throw new FileNotFoundException(definition.Signature.ValidatingCertificate);
                }

                validatingContent = File.ReadAllBytes(validationCertificateFile);
            }

            if (!string.IsNullOrEmpty(definition.Signature.SigningCertificate))
            {
                var signingCertificateFile = SearchCertificate(definition.Signature.SigningCertificate, rootPath);

                if (string.IsNullOrEmpty(signingCertificateFile))
                {
                    throw new FileNotFoundException(definition.Signature.SigningCertificate);
                }

                signingContent = File.ReadAllBytes(signingCertificateFile);
            }

            TokenSecurityType = TokenSecurityTypes.Certificate;
            SetCertificateSignature(validatingContent, signingContent, definition.Signature.SigningCertificatePassword);
        }

        #endregion

        #region Private Methods

        private static string? SearchCertificate(string name, string rootPath)
        {
            return Directory.GetFiles(rootPath, name, SearchOption.AllDirectories).FirstOrDefault();
        }

        private void SetPassPhraseSignature(string passPhrase)
        {
            TokenCertificateAlgorithm = null;
            CertificateAlgorithm = null;

            passPhrase = Convert.ToBase64String(Encoding.UTF8.GetBytes(passPhrase));

            SigningTokenCredential = new TokenCredential
            {
                SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(passPhrase)),
            };

            if (!TokenSecurityAlgorithm.HasValue)
            {
                TokenSecurityAlgorithm = TokenSecurityAlgorithms.HmacSha256;
            }

            SigningTokenCredential.SigningCredentials = new SigningCredentials(SigningTokenCredential.SecurityKey,
                    TokenSecurityAlgorithm.Equals(TokenSecurityAlgorithms.HmacSha256)
                        ? SecurityAlgorithms.HmacSha256
                        : TokenSecurityAlgorithm.Equals(TokenSecurityAlgorithms.HmacSha384)
                            ? SecurityAlgorithms.HmacSha384
                            : TokenSecurityAlgorithm.Equals(TokenSecurityAlgorithms.HmacSha512)
                                ? SecurityAlgorithms.HmacSha512
                                : SecurityAlgorithms.HmacSha256);

            ValidatingTokenCredential = SigningTokenCredential;
        }

        private static string GetCertificateAlgorithm(X509Certificate2 certificate)
        {
            return certificate.SignatureAlgorithm.FriendlyName?.ToUpper() ?? "RSA";
        }

        private void SetCertificateSignature(byte[]? validatingContent, byte[]? signingContent, string? signingCertificatePassword = null)
        {
            TokenCertificateAlgorithm = SecurityAlgorithms.RsaSha256;

            if (validatingContent != null && validatingContent.Length > 0)
            {
                var validatingCertificate = new X509Certificate2(validatingContent);

                ValidatingTokenCredential = new TokenCredential
                {
                    SecurityKey = new X509SecurityKey(validatingCertificate)
                };

                ValidatingTokenCredential.SigningCredentials = new SigningCredentials(ValidatingTokenCredential.SecurityKey, SecurityAlgorithms.RsaSha256);
                CertificateAlgorithm = GetCertificateAlgorithm(validatingCertificate);
            }

            if (signingContent == null || signingContent.Length <= 0)
            {
                return;
            }

            X509Certificate2 signingCertificate;

            if (string.IsNullOrEmpty(signingCertificatePassword))
            {
                signingCertificate = new X509Certificate2(signingContent);
            }
            else
            {
                signingCertificate = new X509Certificate2(signingContent, signingCertificatePassword, X509KeyStorageFlags.Exportable);
            }

            SigningTokenCredential = new TokenCredential
            {
                SecurityKey = new X509SecurityKey(signingCertificate)
            };

            CertificateAlgorithm = GetCertificateAlgorithm(signingCertificate);
            SigningTokenCredential.SigningCredentials = new SigningCredentials(SigningTokenCredential.SecurityKey, SecurityAlgorithms.RsaSha256);
        }

        #endregion
    }
}
