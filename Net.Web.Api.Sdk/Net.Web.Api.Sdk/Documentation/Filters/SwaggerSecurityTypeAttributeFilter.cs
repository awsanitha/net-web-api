using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter. Annotates operation descriptions with security type info.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        #region IOperationFilter Implementation

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityType = GetSecurityType(context);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
                return;
            }

            var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true)
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true) ?? System.Array.Empty<object>())
                .OfType<SwaggerSecurityTypeAttribute>()
                .FirstOrDefault();

            if (attr != null)
            {
                operation.Description = attr.SecurityType;
            }
        }

        #endregion

        #region Private Methods

        private static string? GetSecurityType(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var controllerType = methodInfo.DeclaringType;

            if (controllerType == null)
            {
                return null;
            }

            // Check method-level AllowAnonymous
            if (methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check method-level TokenAuthorize
            var tokenAttr = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .OfType<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            if (tokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);
            }

            // Check method-level BasicAuthorize
            if (methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).Any())
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check controller-level AllowAnonymous
            if (controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check controller-level TokenAuthorize
            var controllerTokenAttr = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .OfType<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            return controllerTokenAttr != null
                ? SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerTokenAttr)
                : SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attribute)
        {
            var audienceString = attribute.IntendedAudiences;
            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }

        #endregion
    }
}
