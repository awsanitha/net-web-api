using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Class TokenNameExistsAttribute. Validates that the token name exists in the configured token definitions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TokenNameExistsAttribute : ValidationAttribute
    {
        #region ValidationAttribute Overrides

        /// <inheritdoc />
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Try to resolve the service from DI context first, fall back to InjectionContainer
            IJwtTokenService? service = null;

            if (validationContext.GetService(typeof(IJwtTokenService)) is IJwtTokenService contextService)
            {
                service = contextService;
            }
            else
            {
                try
                {
                    service = InjectionContainer.Instance.GetService<IJwtTokenService>();
                }
                catch
                {
                    // Service not available
                }
            }

            if (service == null)
            {
                return ValidationResult.Success;
            }

            var tokenName = value?.ToString()?.Trim().ToUpper() ?? string.Empty;
            var exists = service.Tokens.ContainsKey(tokenName);

            if (exists)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(string.Format(Resources.TokenNameNotFound, tokenName));
        }

        #endregion
    }
}
