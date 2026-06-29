using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TokenNameExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Try DI first, fall back to InjectionContainer
            var service = validationContext.GetService<IJwtTokenService>()
                ?? InjectionContainer.Instance.GetService<IJwtTokenService>();

            var tokenName = value?.ToString().Trim().ToUpper() ?? string.Empty;
            if (service != null && service.Tokens.ContainsKey(tokenName))
                return ValidationResult.Success;

            return new ValidationResult(string.Format(Resources.TokenNameNotFound, tokenName));
        }
    }
}
