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
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        /// <inheritdoc />
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
                    .FirstOrDefault() as SwaggerSecurityTypeAttribute;

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

            // Check method-level attributes first
            var allowAnonymous = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();

            if (allowAnonymous != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var tokenAttr = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault() as TokenAuthorizeAttribute;

            if (tokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);
            }

            var basicAttr = methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).FirstOrDefault();

            if (basicAttr != null)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check controller-level attributes
            var controllerType = methodInfo.DeclaringType;

            if (controllerType == null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            allowAnonymous = controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();

            if (allowAnonymous != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            tokenAttr = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault() as TokenAuthorizeAttribute;

            return tokenAttr != null
                ? SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr)
                : SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute tokenAuthorizeAttribute)
        {
            var audienceString = tokenAuthorizeAttribute?.IntendedAudiences;

            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }
    }
}
