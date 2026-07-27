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
    /// Class SwaggerSecurityTypeAttributeFilter.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        #region IOperationFilter Implementations

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
                SwaggerSecurityTypeAttribute attr = context.MethodInfo?.GetCustomAttribute<SwaggerSecurityTypeAttribute>(false);

                if (attr == null && context.ApiDescription.ActionDescriptor is ControllerActionDescriptor cad)
                {
                    attr = cad.ControllerTypeInfo.GetCustomAttribute<SwaggerSecurityTypeAttribute>(true);
                }

                if (attr != null)
                {
                    operation.Description = attr.SecurityType;
                }
            }
        }

        #endregion

        #region Private Methods

        private static string GetSecurityType(OperationFilterContext context)
        {
            if (context.MethodInfo == null)
            {
                return null;
            }

            var methodInfo = context.MethodInfo;

            // Check method-level attributes first
            if (methodInfo.GetCustomAttribute<AllowAnonymousAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var tokenAttr = methodInfo.GetCustomAttribute<TokenAuthorizeAttribute>(true);
            if (tokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);
            }

            if (methodInfo.GetCustomAttribute<BasicAuthorizeAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check controller-level attributes
            if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerAction))
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var controllerType = controllerAction.ControllerTypeInfo.AsType();

            if (controllerType.GetCustomAttribute<AllowAnonymousAttribute>(true) != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var controllerTokenAttr = controllerType.GetCustomAttribute<TokenAuthorizeAttribute>(true);
            if (controllerTokenAttr != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerTokenAttr);
            }

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static string GetIntendedAudiences(TokenAuthorizeAttribute tokenAuthorizeAttribute)
        {
            var audienceString = tokenAuthorizeAttribute?.IntendedAudiences;

            return string.IsNullOrEmpty(audienceString)
                ? string.Empty
                : $"<br/>Intended Audience(s): {audienceString}";
        }

        #endregion
    }
}
