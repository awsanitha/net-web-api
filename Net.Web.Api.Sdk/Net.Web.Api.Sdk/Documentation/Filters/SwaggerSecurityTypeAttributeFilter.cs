using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter.
    /// Sets the operation description based on security attributes.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityType = GetSecurityType(context);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
            }
            else
            {
                var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true)
                    .Cast<SwaggerSecurityTypeAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                {
                    operation.Description = attr.SecurityType;
                }
            }
        }

        private static string GetSecurityType(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;

            if (methodInfo == null)
            {
                return null;
            }

            // Check method-level AllowAnonymous
            var methodAnonymous = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();

            if (methodAnonymous)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check method-level TokenAuthorize
            var tokenAttr = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .Cast<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            if (tokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);
            }

            // Check method-level BasicAuthorize
            var basicAttr = methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).Any();

            if (basicAttr)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check controller-level AllowAnonymous
            var controllerAnonymous = methodInfo.DeclaringType?
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ?? false;

            if (controllerAnonymous)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check controller-level TokenAuthorize
            var controllerTokenAttr = methodInfo.DeclaringType?
                .GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                .Cast<TokenAuthorizeAttribute>()
                .FirstOrDefault();

            if (controllerTokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerTokenAttr);
            }

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attribute)
        {
            var audienceString = attribute?.IntendedAudiences;
            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }
    }
}
