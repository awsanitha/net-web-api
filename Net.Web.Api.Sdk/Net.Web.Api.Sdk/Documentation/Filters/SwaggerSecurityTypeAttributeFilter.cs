using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Sets the security description on a Swagger operation based on security attributes.
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
                var attr = context.MethodInfo.GetCustomAttribute<SwaggerSecurityTypeAttribute>()
                           ?? context.MethodInfo.DeclaringType?.GetCustomAttribute<SwaggerSecurityTypeAttribute>();

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

            // Check action-level AllowAnonymous
            if (methodInfo.GetCustomAttribute<AllowAnonymousAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check action-level TokenAuthorize
            var tokenAttr = methodInfo.GetCustomAttribute<TokenAuthorizeAttribute>(true);
            if (tokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);
            }

            // Check action-level BasicAuthorize
            if (methodInfo.GetCustomAttribute<BasicAuthorizeAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            var controllerType = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)
                ?.ControllerTypeInfo.AsType();

            if (controllerType == null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check controller-level AllowAnonymous
            if (controllerType.GetCustomAttribute<AllowAnonymousAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check controller-level TokenAuthorize
            var controllerTokenAttr = controllerType.GetCustomAttribute<TokenAuthorizeAttribute>(true);
            if (controllerTokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerTokenAttr);
            }

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attribute)
        {
            if (attribute == null || string.IsNullOrEmpty(attribute.IntendedAudiences))
            {
                return string.Empty;
            }

            return $"<br/>Intended Audience(s): {attribute.IntendedAudiences}";
        }
    }
}
