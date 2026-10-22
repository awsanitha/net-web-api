using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;

            if (methodInfo == null)
                return;

            var securityType = GetSecurityType(methodInfo);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
            }
            else
            {
                var attr = methodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true)
                    .Concat(methodInfo.DeclaringType?.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true) ?? Enumerable.Empty<object>())
                    .Cast<SwaggerSecurityTypeAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                    operation.Description = attr.SecurityType;
            }
        }

        private static string GetSecurityType(System.Reflection.MethodInfo methodInfo)
        {
            var declaringType = methodInfo.DeclaringType;

            // Check if anonymous at method level
            if (methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            // Check for TokenAuthorize at method level
            var tokenAttr = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .Cast<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            if (tokenAttr != null)
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);

            // Check for BasicAuthorize at method level
            if (methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).Any())
                return SwaggerSecurityTypeConstants.BASIC_SECURED;

            if (declaringType == null)
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            // Check controller level attributes
            if (declaringType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            tokenAttr = declaringType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .Cast<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            return tokenAttr != null
                ? SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr)
                : SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute tokenAuthorizeAttribute)
        {
            var audienceString = tokenAuthorizeAttribute.IntendedAudiences;
            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }
    }
}
