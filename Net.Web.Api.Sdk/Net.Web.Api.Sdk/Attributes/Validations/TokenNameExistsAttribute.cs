using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Class TokenNameExistsAttribute.
    /// Validates that the token name exists in the token service configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TokenNameExistsAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Resolve IJwtTokenService from the DI container via ValidationContext
            var service = validationContext.GetService<IJwtTokenService>()
                          ?? validationContext.GetRequiredService<IJwtTokenService>();

            var tokenName = value != null ? value.ToString().Trim().ToUpper() : string.Empty;
            var exists = service.Tokens.ContainsKey(tokenName);

            return exists
                ? ValidationResult.Success
                : new ValidationResult(string.Format(Resources.TokenNameNotFound, tokenName));
        }
    }
}
