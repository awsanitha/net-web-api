using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        #region IOperationFilter Implementation

        /// <inheritdoc />
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityType = GetSecurityType(context);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
            }
            else
            {
                var attr = SwaggerFilterHelper.GetControllerAndActionAttributes<SwaggerSecurityTypeAttribute>(context).FirstOrDefault();

                if (attr != null)
                {
                    operation.Description = attr.SecurityType;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the type of the security.
        /// </summary>
        /// <param name="context">The operation filter context.</param>
        /// <returns>System.String.</returns>
        private static string GetSecurityType(OperationFilterContext context)
        {
            var actionMethod = context.MethodInfo
                ?? (context.ApiDescription?.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;

            if (actionMethod == null)
            {
                return null;
            }

            var controllerType = actionMethod.DeclaringType;

            if (controllerType == null)
            {
                return null;
            }

            var customAttribute = GetAttribute<AllowAnonymousAttribute>(actionMethod);

            if (customAttribute != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var tokenAttribute = GetAttribute<TokenAuthorizeAttribute>(actionMethod);

            if (tokenAttribute != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(tokenAttribute);
            }

            var basicAttribute = GetAttribute<BasicAuthorizeAttribute>(actionMethod);

            if (basicAttribute != null)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            var controllerAnonymous = controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();

            if (controllerAnonymous != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            var controllerToken = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault();

            return controllerToken != null
                ? SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(controllerToken)
                : SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        /// <summary>
        /// Gets a custom attribute declared on a member.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="member">The member.</param>
        /// <returns>The attribute, or <c>null</c>.</returns>
        private static T GetAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }

        /// <summary>
        /// Gets the intended audiences.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetIntendedAudiences(object attribute)
        {
            if (!(attribute is TokenAuthorizeAttribute tokenAuthorizeAttribute))
            {
                return string.Empty;
            }

            var audienceString = tokenAuthorizeAttribute.IntendedAudiences;

            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }

        #endregion
    }
}
