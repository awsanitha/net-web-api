using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Adds a security type description to each operation based on authorization attributes.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityType = GetSecurityType(context);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
                return;
            }

            var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), false)
                              .FirstOrDefault() as SwaggerSecurityTypeAttribute
                    ?? context.MethodInfo.DeclaringType?
                              .GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), false)
                              .FirstOrDefault() as SwaggerSecurityTypeAttribute;

            if (attr != null) operation.Description = attr.SecurityType;
        }

        private static string GetSecurityType(OperationFilterContext context)
        {
            var method = context.MethodInfo;
            var controllerType = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)
                                 ?.ControllerTypeInfo.AsType();

            // Check method-level attributes first
            if (method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            var tokenAttr = method.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                                  .FirstOrDefault() as TokenAuthorizeAttribute;
            if (tokenAttr != null)
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);

            if (method.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).Any())
                return SwaggerSecurityTypeConstants.BASIC_SECURED;

            // Fall back to controller-level
            if (controllerType == null) return SwaggerSecurityTypeConstants.ANONYMOUS;

            if (controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            var controllerTokenAttr = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true)
                                                     .FirstOrDefault() as TokenAuthorizeAttribute;
            if (controllerTokenAttr != null)
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerTokenAttr);

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attr)
        {
            return string.IsNullOrEmpty(attr.IntendedAudiences)
                ? string.Empty
                : $"<br/>Intended Audience(s): {attr.IntendedAudiences}";
        }
    }
}
