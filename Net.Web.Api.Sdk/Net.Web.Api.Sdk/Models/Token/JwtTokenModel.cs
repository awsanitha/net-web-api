using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Net.Web.Api.Sdk.Configurations.Token;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Net.Web.Api.Sdk.Models.Token
{
    internal enum TokenInternalClaimNames
    {
        nbf, exp, iat, iss, aud, jti, exm, tn, otu
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenSecurityTypes { PassPhrase, Certificate }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenSecurityAlgorithms { HmacSha256, HmacSha384, HmacSha512 }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenStatus { AlreadyUsed, Expired, Invalid, InvalidAudience, TokenRequired, Revoked, Valid }

    internal interface ITokenCredential
    {
        SecurityKey SecurityKey { get; set; }
        SigningCredentials SigningCredentials { get; set; }
    }

    /// <summary>
    /// Represents a configured JWT token definition.
    /// </summary>
    public class JwtTokenModel
    {
        private const string URN_PATTERN =
            @"^(?<URN>[uU][rR][nN]\:(?<NID>(?!urn\:)[a-zA-Z0-9][a-zA-Z0-9-]{1,31})\:(?<NSS>([a-zA-Z0-9()+,._!*':=@;$-]|%[0-9a-fA-F]{2})+))$";

        public string TokenName { get; }
        public string TokenIssuer { get; }
        public string TokenIntendedAudience { get; }
        public double TokenExpirationInMinutes { get; }
        public bool IsTokenBase64Encoded { get; }
        public bool OneTimeUse { get; }
        public TokenSecurityTypes TokenSecurityType { get; }
        public TokenSecurityAlgorithms? TokenSecurityAlgorithm { get; set; }

        [JsonIgnore]
        internal ITokenCredential SigningTokenCredential { get; set; }

        [JsonIgnore]
        internal ITokenCredential ValidatingTokenCredential { get; set; }

        public string TokenCertificateAlgorithm { get; internal set; }
        public string CertificateAlgorithm { get; internal set; }

        public bool ShouldSerializeTokenSecurityAlgorithm() => TokenSecurityAlgorithm.HasValue;
        public bool ShouldSerializeTokenCertificateAlgorithm() => !string.IsNullOrEmpty(TokenCertificateAlgorithm);
        public bool ShouldSerializeCertificateAlgorithm() => !string.IsNullOrEmpty(CertificateAlgorithm);

        internal class TokenCredential : ITokenCredential
        {
            public SecurityKey SecurityKey { get; set; }
            public SigningCredentials SigningCredentials { get; set; }
        }

        [SuppressMessage("ReSharper", "NotResolvedInText")]
        public JwtTokenModel(string tokenName, TokenDefinitionElement definition, string rootPath)
        {
            TokenName = tokenName.ToUpper();

            if (!Uri.TryCreate(definition.Issuer, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException(definition.Issuer);

            TokenIssuer = definition.Issuer;

            if (!Regex.IsMatch(definition.IntendedAudience, URN_PATTERN, RegexOptions.IgnorePatternWhitespace))
                throw new ArgumentException(definition.IntendedAudience);

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

            if (string.IsNullOrEmpty(definition.Signature.ValidatingCertificate) &&
                string.IsNullOrEmpty(definition.Signature.SigningCertificate))
                throw new ArgumentNullException("ValidatingCertificate");

            byte[] validatingContent = null;
            byte[] signingContent = null;

            if (!string.IsNullOrEmpty(definition.Signature.ValidatingCertificate))
            {
                var certFile = SearchCertificate(definition.Signature.ValidatingCertificate, rootPath);
                if (string.IsNullOrEmpty(certFile)) throw new FileNotFoundException(definition.Signature.ValidatingCertificate);
                validatingContent = File.ReadAllBytes(certFile);
            }

            if (!string.IsNullOrEmpty(definition.Signature.SigningCertificate))
            {
                var certFile = SearchCertificate(definition.Signature.SigningCertificate, rootPath);
                if (string.IsNullOrEmpty(certFile)) throw new FileNotFoundException(definition.Signature.SigningCertificate);
                signingContent = File.ReadAllBytes(certFile);
            }

            TokenSecurityType = TokenSecurityTypes.Certificate;
            SetCertificateSignature(validatingContent, signingContent, definition.Signature.SigningCertificatePassword);
        }

        private static string SearchCertificate(string name, string rootPath)
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
                SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(passPhrase))
            };

            if (!TokenSecurityAlgorithm.HasValue)
                TokenSecurityAlgorithm = TokenSecurityAlgorithms.HmacSha256;

            var algorithm = TokenSecurityAlgorithm == TokenSecurityAlgorithms.HmacSha384
                ? SecurityAlgorithms.HmacSha384
                : TokenSecurityAlgorithm == TokenSecurityAlgorithms.HmacSha512
                    ? SecurityAlgorithms.HmacSha512
                    : SecurityAlgorithms.HmacSha256;

            SigningTokenCredential.SigningCredentials =
                new SigningCredentials(SigningTokenCredential.SecurityKey, algorithm);

            ValidatingTokenCredential = SigningTokenCredential;
        }

        private void SetCertificateSignature(byte[] validatingContent, byte[] signingContent,
            string signingCertificatePassword = null)
        {
            TokenCertificateAlgorithm = SecurityAlgorithms.RsaSha256;

            if (validatingContent != null && validatingContent.Length > 0)
            {
                var cert = new X509Certificate2(validatingContent);
                ValidatingTokenCredential = new TokenCredential
                {
                    SecurityKey = new X509SecurityKey(cert),
                    SigningCredentials = new SigningCredentials(new X509SecurityKey(cert), SecurityAlgorithms.RsaSha256)
                };
                CertificateAlgorithm = cert.SignatureAlgorithm.FriendlyName?.ToUpper();
            }

            if (signingContent == null || signingContent.Length == 0) return;

            var signingCert = string.IsNullOrEmpty(signingCertificatePassword)
                ? new X509Certificate2(signingContent)
                : new X509Certificate2(signingContent, signingCertificatePassword, X509KeyStorageFlags.Exportable);

            SigningTokenCredential = new TokenCredential
            {
                SecurityKey = new X509SecurityKey(signingCert),
                SigningCredentials = new SigningCredentials(new X509SecurityKey(signingCert), SecurityAlgorithms.RsaSha256)
            };
            CertificateAlgorithm = signingCert.SignatureAlgorithm.FriendlyName?.ToUpper();
        }
    }
}
