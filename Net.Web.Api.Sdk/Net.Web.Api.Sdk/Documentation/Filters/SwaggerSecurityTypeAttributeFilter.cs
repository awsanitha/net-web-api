using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Operation filter that appends a human-readable security description to the Swagger
    /// operation summary based on the authorization attributes present.
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
                var attr = context.MethodInfo
                    ?.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), true)
                    .OfType<SwaggerSecurityTypeAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                    operation.Description = attr.SecurityType;
            }
        }

        private static string GetSecurityType(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            if (methodInfo == null) return null;

            // Check method-level attributes first
            if (HasAttribute<AllowAnonymousAttribute>(methodInfo))
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            var tokenAttr = methodInfo.GetCustomAttribute<TokenAuthorizeAttribute>();
            if (tokenAttr != null)
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);

            if (HasAttribute<BasicAuthorizeAttribute>(methodInfo))
                return SwaggerSecurityTypeConstants.BASIC_SECURED;

            // Fall back to controller-level attributes
            var controllerType = methodInfo.DeclaringType;
            if (controllerType == null) return null;

            if (HasAttribute<AllowAnonymousAttribute>(controllerType))
                return SwaggerSecurityTypeConstants.ANONYMOUS;

            tokenAttr = controllerType.GetCustomAttribute<TokenAuthorizeAttribute>();
            if (tokenAttr != null)
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttr);

            return SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        private static bool HasAttribute<T>(MemberInfo memberInfo) where T : System.Attribute
            => memberInfo.GetCustomAttributes(typeof(T), true).Any();

        private static string GetIntendedAudiences(TokenAuthorizeAttribute attr)
        {
            return string.IsNullOrEmpty(attr.IntendedAudiences)
                ? string.Empty
                : $"<br/>Intended Audience(s): {attr.IntendedAudiences}";
        }
    }
}
