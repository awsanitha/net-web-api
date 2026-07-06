using Net.Web.Api.Sdk.Injection.Containers;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Class TokenNameExistsAttribute.
    /// Validates that the token name exists in the configured token service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TokenNameExistsAttribute : ValidationAttribute
    {
        #region ValidationAttribute Overrides

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Try to get service from validation context first (preferred for ASP.NET Core DI)
            var service = validationContext.GetService(typeof(IJwtTokenService)) as IJwtTokenService
                         ?? InjectionContainer.Instance.GetService<IJwtTokenService>();

            if (service == null)
            {
                return ValidationResult.Success;
            }

            var tokenName = value != null ? value.ToString().Trim().ToUpper() : string.Empty;
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
