using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Class TokenNameExistsAttribute. Validates that a token name exists in the configured tokens.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TokenNameExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var service = (IJwtTokenService)validationContext.GetService(typeof(IJwtTokenService));
            var tokenName = value != null ? value.ToString().Trim().ToUpper() : string.Empty;
            var exists = service?.Tokens?.ContainsKey(tokenName) ?? false;

            if (exists)
                return ValidationResult.Success;

            return new ValidationResult(string.Format(Resources.TokenNameNotFound, tokenName));
        }
    }
}
