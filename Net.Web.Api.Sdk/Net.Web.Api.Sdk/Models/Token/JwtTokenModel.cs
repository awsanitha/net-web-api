using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Configurations.Token;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Net.Web.Api.Sdk.Models.Token
{
    /// <summary>
    /// Class JwtTokenModel.
    /// </summary>
    public class JwtTokenModel
    {
        #region Static Properties

        /// <summary>
        /// The content root path, set during application startup.
        /// </summary>
        public static string ContentRootPath { get; set; }

        #endregion

        #region Enums

        /// <summary>
        /// Enum TokenSecurityTypes
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
            /// The HMAC sha256
            /// </summary>
            HmacSha256,

            /// <summary>
            /// The HMAC sha384
            /// </summary>
            HmacSha384,

            /// <summary>
            /// The HMAC sha512
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
            /// The token required
            /// </summary>
            TokenRequired,

            /// <summary>
            /// The invalid
            /// </summary>
            Invalid,

            /// <summary>
            /// The expired
            /// </summary>
            Expired,

            /// <summary>
            /// The invalid audience
            /// </summary>
            InvalidAudience,

            /// <summary>
            /// The revoked
            /// </summary>
            Revoked,

            /// <summary>
            /// The already used
            /// </summary>
            AlreadyUsed
        }

        /// <summary>
        /// Enum TokenInternalClaimNames
        /// </summary>
        public enum TokenInternalClaimNames
        {
            /// <summary>
            /// Not before
            /// </summary>
            nbf,

            /// <summary>
            /// Expiration
            /// </summary>
            exp,

            /// <summary>
            /// Issued at
            /// </summary>
            iat,

            /// <summary>
            /// Issuer
            /// </summary>
            iss,

            /// <summary>
            /// Audience
            /// </summary>
            aud,

            /// <summary>
            /// Token name
            /// </summary>
            tn,

            /// <summary>
            /// JWT ID
            /// </summary>
            jti,

            /// <summary>
            /// Expiration minutes
            /// </summary>
            exm,

            /// <summary>
            /// One time use
            /// </summary>
            otu
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        /// <value>The name of the token.</value>
        [JsonProperty("tokenName")]
        public string TokenName { get; set; }

        /// <summary>
        /// Gets or sets the token issuer.
        /// </summary>
        /// <value>The token issuer.</value>
        [JsonProperty("tokenIssuer")]
        public string TokenIssuer { get; set; }

        /// <summary>
        /// Gets or sets the token intended audience.
        /// </summary>
        /// <value>The token intended audience.</value>
        [JsonProperty("tokenIntendedAudience")]
        public string TokenIntendedAudience { get; set; }

        /// <summary>
        /// Gets or sets the token expiration in minutes.
        /// </summary>
        /// <value>The token expiration in minutes.</value>
        [JsonProperty("tokenExpirationInMinutes")]
        public double TokenExpirationInMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is token base64 encoded.
        /// </summary>
        /// <value><c>true</c> if this instance is token base64 encoded; otherwise, <c>false</c>.</value>
        [JsonProperty("isTokenBase64Encoded")]
        public bool IsTokenBase64Encoded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [one time use].
        /// </summary>
        /// <value><c>true</c> if [one time use]; otherwise, <c>false</c>.</value>
        [JsonProperty("oneTimeUse")]
        public bool OneTimeUse { get; set; }

        /// <summary>
        /// Gets the type of the token security.
        /// </summary>
        /// <value>The type of the token security.</value>
        [JsonIgnore]
        public TokenSecurityTypes? TokenSecurityType { get; private set; }

        /// <summary>
        /// Gets the token security algorithm.
        /// </summary>
        /// <value>The token security algorithm.</value>
        [JsonIgnore]
        public TokenSecurityAlgorithms? TokenSecurityAlgorithm { get; private set; }

        /// <summary>
        /// Gets the token certificate algorithm.
        /// </summary>
        /// <value>The token certificate algorithm.</value>
        [JsonIgnore]
        public string TokenCertificateAlgorithm { get; private set; }

        /// <summary>
        /// Gets the certificate algorithm.
        /// </summary>
        /// <value>The certificate algorithm.</value>
        [JsonIgnore]
        public string CertificateAlgorithm { get; private set; }

        /// <summary>
        /// Gets the signing token credential.
        /// </summary>
        /// <value>The signing token credential.</value>
        [JsonIgnore]
        public TokenCredential SigningTokenCredential { get; private set; }

        /// <summary>
        /// Gets the validating token credential.
        /// </summary>
        /// <value>The validating token credential.</value>
        [JsonIgnore]
        public TokenCredential ValidatingTokenCredential { get; private set; }

        #endregion

        #region Inner Classes

        /// <summary>
        /// Class TokenCredential.
        /// </summary>
        public class TokenCredential
        {
            /// <summary>
            /// Gets or sets the security key.
            /// </summary>
            /// <value>The security key.</value>
            public SecurityKey SecurityKey { get; set; }

            /// <summary>
            /// Gets or sets the signing credentials.
            /// </summary>
            /// <value>The signing credentials.</value>
            public SigningCredentials SigningCredentials { get; set; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenModel"/> class.
        /// </summary>
        public JwtTokenModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenModel"/> class.
        /// </summary>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="definition">The definition.</param>
        public JwtTokenModel(string tokenName, TokenDefinitionElement definition)
        {
            TokenName = tokenName.ToUpper();
            TokenIssuer = definition.Issuer;
            TokenIntendedAudience = definition.IntendedAudience;
            TokenExpirationInMinutes = definition.ExpirationInMinute;
            IsTokenBase64Encoded = definition.IsBase64Encoded;
            OneTimeUse = definition.OneTimeUse;

            TokenSecurityAlgorithm = TokenSecurityAlgorithms.HmacSha256;

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

            byte[] validatingContent = null;
            byte[] signingContent = null;

            if (!string.IsNullOrEmpty(definition.Signature.ValidatingCertificate))
            {
                var validationCertificateFile = SearchCertificate(definition.Signature.ValidatingCertificate);

                if (string.IsNullOrEmpty(validationCertificateFile))
                {
                    throw new FileNotFoundException(definition.Signature.ValidatingCertificate);
                }

                validatingContent = File.ReadAllBytes(validationCertificateFile);
            }

            if (!string.IsNullOrEmpty(definition.Signature.SigningCertificate))
            {
                var signingCertificateFile = SearchCertificate(definition.Signature.SigningCertificate);

                signingContent = File.ReadAllBytes(signingCertificateFile);

                if (string.IsNullOrEmpty(signingCertificateFile))
                {
                    throw new FileNotFoundException(definition.Signature.SigningCertificate);
                }
            }

            TokenSecurityType = TokenSecurityTypes.Certificate;

            SetCertificateSignature(validatingContent, signingContent, definition.Signature.SigningCertificatePassword);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Searches the certificate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private static string SearchCertificate(string name)
        {
            var rootPath = ContentRootPath ?? AppContext.BaseDirectory;
            var certificate = Directory.GetFiles(rootPath, name, SearchOption.AllDirectories).FirstOrDefault();

            return certificate;
        }

        /// <summary>
        /// Sets the pass phrase signature.
        /// </summary>
        /// <param name="passPhrase">The pass phrase.</param>
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

        /// <summary>
        /// Gets the certificate algorithm.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns>System.String.</returns>
        private static string GetCertificateAlgorithm(X509Certificate2 certificate)
        {
            return certificate.SignatureAlgorithm.FriendlyName.ToUpper();
        }

        /// <summary>
        /// Sets the certificate signature.
        /// </summary>
        /// <param name="validatingContent">Content of the validating.</param>
        /// <param name="signingContent">Content of the signing.</param>
        /// <param name="signingCertificatePassword">The signing certificate password.</param>
        private void SetCertificateSignature(byte[] validatingContent, byte[] signingContent, string signingCertificatePassword = null)
        {
            TokenCertificateAlgorithm = SecurityAlgorithms.RsaSha256;

            if (validatingContent != null && validatingContent.Length > 0)
            {
                var validatingCertificate = X509CertificateLoader.LoadCertificate(validatingContent);

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

            if (signingCertificatePassword == null)
            {
                signingCertificate = X509CertificateLoader.LoadCertificate(signingContent);
            }
            else
            {
                signingCertificate = X509CertificateLoader.LoadPkcs12(signingContent, signingCertificatePassword, X509KeyStorageFlags.Exportable);
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
