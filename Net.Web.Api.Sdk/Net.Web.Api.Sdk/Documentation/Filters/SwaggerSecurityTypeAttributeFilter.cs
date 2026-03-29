using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter.
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
            }
        }

        #endregion

        #region Private Methods

        private static string GetSecurityType(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var controllerType = methodInfo?.DeclaringType;

            if (methodInfo == null || controllerType == null)
            {
                return null;
            }

            // Check method-level attributes first
            var allowAnonymousOnMethod = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();

            if (allowAnonymousOnMethod)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var tokenAttrOnMethod = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault() as TokenAuthorizeAttribute;

            if (tokenAttrOnMethod != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttrOnMethod);
            }

            var basicAttrOnMethod = methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).Any();

            if (basicAttrOnMethod)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check controller-level attributes
            var allowAnonymousOnController = controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();

            if (allowAnonymousOnController)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var tokenAttrOnController = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault() as TokenAuthorizeAttribute;

            if (tokenAttrOnController != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttrOnController);
            }

            // Check custom SwaggerSecurityTypeAttribute
            var attr = methodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true).FirstOrDefault() as SwaggerSecurityTypeAttribute
                ?? controllerType.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true).FirstOrDefault() as SwaggerSecurityTypeAttribute;

            if (attr != null)
            {
                return attr.SecurityType;
            }

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attribute)
        {
            var audienceString = attribute?.IntendedAudiences;

            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }

        #endregion
    }
}
